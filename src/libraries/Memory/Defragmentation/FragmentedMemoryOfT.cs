using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ofella.Utilities.Memory.Defragmentation;

/// <summary>
/// Abstracts multiple chunks of <see cref="Memory{T}"/> as a single contiguous region, without copying them anywhere.
/// </summary>
/// <typeparam name="T">The type of the elements in the <see cref="FragmentedMemory{T}"/>.</typeparam>
public readonly struct FragmentedMemory<T> : IDisposable
{
    private readonly MemoryFragment<T>[] _fragments;
    private readonly int _fragmentCount;
    private readonly int _offset;
    private readonly FragmentedPosition _endOfStreamPosition;

    /// <summary>
    /// The sum of the length of the instance's fragments.
    /// </summary>
    public int Length { get; init; }

    /// <summary>
    /// Creates an instance of <see cref="FragmentedMemory{T}"/> by providing its <paramref name="fragments"/> as instances of <see cref="Memory{T}"/>.
    /// </summary>
    /// <param name="fragments">Fragments of memory as <see cref="Memory{T}"/> to abstract as a single contiguous region.</param>
    public FragmentedMemory(Memory<T>[] fragments)
    {
        _fragments = ArrayPool<MemoryFragment<T>>.Shared.Rent(fragments.Length);
        _fragmentCount = fragments.Length;
        _offset = 0;
        _endOfStreamPosition = new FragmentedPosition(fragments.Length, 0);

        int calculatedOffset = 0;

        for (int i = 0; i < fragments.Length; ++i)
        {
            _fragments[i] = new(fragments[i], calculatedOffset);
            calculatedOffset += fragments[i].Length;
        }

        Length = calculatedOffset;
    }

    /// <summary>
    /// Creates an instance of <see cref="FragmentedMemory{T}"/> by slicing another instance using an <paramref name="offset"/> and a <paramref name="length"/>.
    /// </summary>
    /// <param name="fragmentedMemory">The instance to slice.</param>
    /// <param name="offset">The (inclusive) offset to slice from.</param>
    /// <param name="length">The length of the slice.</param>
    public FragmentedMemory(in FragmentedMemory<T> fragmentedMemory, int offset, int length)
    {
        _fragments = fragmentedMemory._fragments;
        _fragmentCount = fragmentedMemory._fragmentCount;
        _offset = offset;
        _endOfStreamPosition = fragmentedMemory._endOfStreamPosition;
        Length = length;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FragmentedMemory<T> Slice(int offset, int length) => new(in this, _offset + offset, length);

    public FragmentedMemory<T> this[Range range] => Slice(_offset + range.Start.Value, range.End.Value - range.Start.Value);


    public FragmentedPosition CopyTo(T[] destination, FragmentedPosition? startingPositionOverride)
    {
        return CopyTo(ref destination[0], startingPositionOverride);
    }

    public FragmentedPosition CopyTo(Memory<T> destination, FragmentedPosition? startingPositionOverride)
    {
        return CopyTo(ref destination.Span[0], startingPositionOverride);
    }

    public FragmentedPosition CopyTo(Span<T> destination, FragmentedPosition? startingPositionOverride)
    {
        return CopyTo(ref destination[0], startingPositionOverride);
    }

    public async ValueTask<FragmentedPosition> CopyToAsync(Stream destination, FragmentedPosition? startingPositionOverride, CancellationToken cancellationToken = default)
    {
        if (_fragments is not MemoryFragment<byte>[] fragmentsOfByte)
        {
            throw new NotSupportedException("This method can only be used when the fragments are of type Memory<byte>, since the Stream type only supports byte streams.");
        }

        var startingPosition = startingPositionOverride ?? GetFragmentedPosition(_offset);

        if (startingPosition == FragmentedPosition.NotFound)
        {
            goto EndOfStream;
        }

        // Copy from first fragment: it needs special handling because it doesn't necessarily start from 0.
        var firstFragmentMemory = fragmentsOfByte[startingPosition.FragmentNo].Memory[startingPosition.Offset..];
        int bytesToCopy = Math.Min(firstFragmentMemory.Length, Length);
        await destination.WriteAsync(firstFragmentMemory[..bytesToCopy], cancellationToken);

        int currentFragmentNo;
        int destinationOffset = bytesToCopy;

        // Copy from subsequent fragments if needed.
        for (currentFragmentNo = startingPosition.FragmentNo + 1;
            destinationOffset < Length;
            ++currentFragmentNo, destinationOffset += bytesToCopy)
        {
            bytesToCopy = Math.Min(fragmentsOfByte[currentFragmentNo].Memory.Length, Length - destinationOffset);
            await destination.WriteAsync(fragmentsOfByte[currentFragmentNo].Memory[..bytesToCopy], cancellationToken);
        }

        return new(currentFragmentNo, bytesToCopy);

    EndOfStream:
        return new(_fragmentCount, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal FragmentedPosition CopyTo(ref T destination, FragmentedPosition? startingPositionOverride)
    {
        var startingPosition = startingPositionOverride ?? GetFragmentedPosition(_offset);

        if (startingPosition == FragmentedPosition.NotFound || startingPosition == _endOfStreamPosition)
        {
            goto EndOfStream;
        }

        // Copy from first fragment: it needs special handling because it doesn't necessarily start from 0.
        var firstFragmentMemory = _fragments[startingPosition.FragmentNo].Memory[startingPosition.Offset..];
        int bytesToCopy = Math.Min(firstFragmentMemory.Length, Length);

        UnsafeCopyBlock(ref destination, ref firstFragmentMemory.Span[0], bytesToCopy);

        int currentFragmentNo = startingPosition.FragmentNo + 1;
        int destinationOffset = bytesToCopy;

        // Copy from subsequent fragments if needed.
        for (;
            destinationOffset < Length;
            ++currentFragmentNo, destinationOffset += bytesToCopy)
        {
            bytesToCopy = Math.Min(_fragments[currentFragmentNo].Memory.Length, Length - destinationOffset);
            UnsafeCopyBlock(ref destination, destinationOffset, ref _fragments[currentFragmentNo].Memory.Span[..bytesToCopy][0], bytesToCopy);
        }

        var currentPosition = (startingPosition.Offset & (((bytesToCopy - destinationOffset) >>> 31) - 1)) + bytesToCopy;

        return _fragments[currentFragmentNo - 1].Memory.Length != currentPosition
            ? new(currentFragmentNo - 1, currentPosition)
            : new(currentFragmentNo, 0);

    EndOfStream:
        return _endOfStreamPosition;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UnsafeCopyBlock(ref T destination, ref T source, int byteCount)
    {
        ref byte pDestination = ref Unsafe.As<T, byte>(ref destination);
        ref byte pSource = ref Unsafe.As<T, byte>(ref source);

        Unsafe.CopyBlock(ref pDestination, ref pSource, (uint)byteCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UnsafeCopyBlock<TDest, TSrc>(ref TDest destination, int destinationOffset, ref TSrc source, int byteCount)
    {
        ref byte pDestination = ref Unsafe.As<TDest, byte>(ref Unsafe.Add(ref destination, destinationOffset));
        ref byte pSource = ref Unsafe.As<TSrc, byte>(ref source);

        Unsafe.CopyBlock(ref pDestination, ref pSource, (uint)byteCount);
    }

    /// <summary>
    /// Optimized for moving to a specific offset using binary search. This method has a performance of O(log n).
    /// </summary>
    /// <param name="offset">The offset to move to.</param>
    private FragmentedPosition GetFragmentedPosition(int offset)
    {
        if (offset == 0)
        {
            return FragmentedPosition.Beginning;
        }

        ref var fragmentsPtr = ref MemoryMarshal.GetArrayDataReference(_fragments);

        int lowerBoundary = 0;
        int upperBoundary = _fragmentCount - 1;
        int fragmentNoToCheck;

        while (lowerBoundary <= upperBoundary)
        {
            fragmentNoToCheck = (lowerBoundary + upperBoundary) >> 1;
            var fragmentToCheck = Unsafe.Add(ref fragmentsPtr, fragmentNoToCheck);

            if (offset < fragmentToCheck.Offset)
            {
                upperBoundary = fragmentNoToCheck - 1;
            }
            else if (offset > fragmentToCheck.Offset + fragmentToCheck.Memory.Length - 1)
            {
                lowerBoundary = fragmentNoToCheck + 1;
            }
            else
            {
                return new(fragmentNoToCheck, offset - _fragments[fragmentNoToCheck].Offset);
            }
        }

        return FragmentedPosition.NotFound;
    }

    public void Dispose()
    {
        ArrayPool<MemoryFragment<T>>.Shared.Return(_fragments);
    }
}
