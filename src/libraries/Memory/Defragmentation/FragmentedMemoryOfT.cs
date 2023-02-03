using Ofella.Utilities.Memory.ManagedPointers;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ofella.Utilities.Memory.Defragmentation;

/// <summary>
/// Abstracts multiple instances of <see cref="Memory{T}"/> or <see cref="T:Byte[]"/> as a single contiguous sequence, without copying them anywhere.
/// </summary>
/// <typeparam name="T">The type of the elements in the <see cref="FragmentedMemory{T}"/>.</typeparam>
public readonly struct FragmentedMemory<T> : IDisposable
{
    #region Fields & Properties

    private readonly MemoryFragment<T>[] _fragments; // Keeps track of the offsets of the fragments, so that they can be binary searched.
    private readonly int _fragmentCount; // Used for boundary checking, since _fragments comes from an ArrayPool which probably causing its size to be greater than requested.
    private readonly int _offset; // The starting offset when an original instance is sliced.
    private readonly FragmentedPosition _fragmentedPosition; // The starting offset as FragmentedPosition when an original instance is sliced.

    // This property is not a field to decrease the size of the FragmentedMemory<T> structure.
    // Since the values are actually readonly (both the input and the ones in the struct), it never gets allocated.
    private FragmentedPosition EndOfStreamPosition => new(_fragmentCount, 0);

    /// <summary>
    /// The sum of the length of the instance's fragments.
    /// </summary>
    public int Length { get; init; }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates an instance of <see cref="FragmentedMemory{T}"/> by providing its <paramref name="memories"/> as instances of <see cref="Memory{T}"/>.
    /// </summary>
    /// <param name="memories">Fragments of memory as <see cref="Memory{T}"/> to abstract as a single contiguous sequence.</param>
    public FragmentedMemory(Memory<T>[] memories)
    {
        _fragments = ArrayPool<MemoryFragment<T>>.Shared.Rent(memories.Length);
        _fragmentCount = memories.Length;
        _offset = 0;
        _fragmentedPosition = FragmentedPosition.Beginning;

        ref var memory = ref Ptr.Get(memories);
        ref var memoriesBoundary = ref Unsafe.Add(ref memory, memories.Length);
        ref var fragment = ref Ptr.Get(_fragments);
        ref var fragmentsBoundary = ref Unsafe.Add(ref fragment, _fragments.Length);

        for (;
            Unsafe.IsAddressLessThan(ref memory, ref memoriesBoundary);
            memory = ref Unsafe.Add(ref memory, 1), fragment = ref Unsafe.Add(ref fragment, 1))
        {
            fragment = new(memory, Length);
            Length += memory.Length; // Length is calculated by incrementing it by the current memory's length, there Length is the current fragment's offset too.
        }
    }

    /// <summary>
    /// Creates an instance of <see cref="FragmentedMemory{T}"/> by providing its <paramref name="arrays"/> as array of <see cref="T:T[]"/>.
    /// </summary>
    /// <param name="arrays">Fragments of memory as <see cref="T:T[]"/> to abstract as a single contiguous sequence.</param>
    public FragmentedMemory(T[][] arrays)
    {
        _fragments = ArrayPool<MemoryFragment<T>>.Shared.Rent(arrays.Length);
        _fragmentCount = arrays.Length;
        _offset = 0;
        _fragmentedPosition = FragmentedPosition.Beginning;

        ref T[] array = ref Ptr.Get(arrays);
        ref T[] arraysBoundary = ref Unsafe.Add(ref array, arrays.Length);
        ref var fragment = ref Ptr.Get(_fragments);
        ref var fragmentsBoundary = ref Unsafe.Add(ref fragment, _fragments.Length);

        for (;
            Unsafe.IsAddressLessThan(ref array, ref arraysBoundary);
            array = ref Unsafe.Add(ref array, 1), fragment = ref Unsafe.Add(ref fragment, 1))
        {
            fragment = new(array, Length);
            Length += array.Length; // Length is calculated by incrementing it by the current array's length, there Length is the current fragment's offset too.
        }
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
        _fragmentedPosition = FragmentedPosition.NotFound;
        Length = length;
    }

    /// <summary>
    /// Creates an instance of <see cref="FragmentedMemory{T}"/> by slicing another instance using an <paramref name="fragmentedPosition"/> and a <paramref name="length"/>.
    /// </summary>
    /// <param name="fragmentedMemory">The instance to slice.</param>
    /// <param name="fragmentedPosition">The (inclusive) <see cref="FragmentedPosition"/> to slice from.</param>
    /// <param name="length">The length of the slice.</param>
    public FragmentedMemory(in FragmentedMemory<T> fragmentedMemory, FragmentedPosition fragmentedPosition, int length)
    {
        _fragments = fragmentedMemory._fragments;
        _fragmentCount = fragmentedMemory._fragmentCount;
        _offset = -1;
        _fragmentedPosition = fragmentedPosition;
        Length = length;
    }

    public void Dispose()
    {
        ArrayPool<MemoryFragment<T>>.Shared.Return(_fragments);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Forms a slice of the specified length out of the current <see cref="FragmentedMemory{T}"/> starting at the specified index.
    /// </summary>
    /// <param name="offset">The offset at which to begin the slice.</param>
    /// <param name="length">The desired length of the slice.</param>
    /// <returns>A <see cref="FragmentedMemory{T}"/> of <paramref name="length"/> elements starting at <paramref name="offset"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FragmentedMemory<T> Slice(int offset, int length) => new(in this, _offset + offset, length);

    /// <summary>
    /// Forms a slice of the specified length out of the current <see cref="FragmentedMemory{T}"/> starting at the specified <see cref="FragmentedPosition"/>.
    /// </summary>
    /// <param name="offset">The <see cref="FragmentedPosition"/> at which to begin the slice.</param>
    /// <param name="length">The desired length of the slice.</param>
    /// <returns>A <see cref="FragmentedMemory{T}"/> of <paramref name="length"/> elements starting at <paramref name="offset"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FragmentedMemory<T> Slice(FragmentedPosition fragmentedPosition, int length)
    {
        // Disallow this type of slicing when the instance is already sliced by offset.
        if (_offset <= 0) // Favor this branch when BPU has not enough information.
        {
            return new(in this, fragmentedPosition, length);
        }

        throw new InvalidOperationException("FragmentedPosition can only be used to form slices when the FragmentedMemory is unsliced or has previously been sliced by FragmentedPosition too.");
    }

    /// <summary>
    /// Copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by the provided <see cref="T:T[]"/>.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedPosition"/>.</returns>
    public FragmentedPosition CopyTo(T[] destination) => CopyTo(ref Ptr.Get(destination));

    /// <summary>
    /// Copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by the provided <see cref="Memory{T}"/>.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedPosition"/>.</returns>
    public FragmentedPosition CopyTo(Memory<T> destination) => CopyTo(ref Ptr.Get(destination.Span));

    /// <summary>
    /// Copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by the provided <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedPosition"/>.</returns>
    public FragmentedPosition CopyTo(Span<T> destination) => CopyTo(ref Ptr.Get(destination));

    /// <summary>
    /// Asynchronously copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by a managed pointer.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedPosition"/>.</returns>
    /// <exception cref="NotSupportedException"></exception>
    public async ValueTask<FragmentedPosition> CopyToAsync(Stream destination, CancellationToken cancellationToken = default)
    {
        // This is a JIT compile time check, no branching instruction will be emitted.
        if (_fragments is not MemoryFragment<byte>[] fragmentsOfByte)
        {
            throw new NotSupportedException("This method can only be used when the fragments are of type Memory<byte>, since the Stream type only supports byte streams.");
        }

        var startingPosition = _fragmentedPosition != FragmentedPosition.NotFound
            ? _fragmentedPosition
            : GetFragmentedPosition(_offset);

        if (startingPosition == FragmentedPosition.NotFound || startingPosition == EndOfStreamPosition)
        {
            goto EndOfStream;
        }

        // Copy from first fragment: it needs special handling because it doesn't necessarily start from 0.
        var firstFragmentMemory = fragmentsOfByte[startingPosition.FragmentNo].Memory[startingPosition.Offset..];
        int copyCount = Math.Min(firstFragmentMemory.Length, Length);

        await destination.WriteAsync(firstFragmentMemory, cancellationToken);

        int currentFragmentNo = startingPosition.FragmentNo + 1;
        int destinationOffset = copyCount;

        // Copy from subsequent fragments if needed.
        for (;
            destinationOffset < Length;
            ++currentFragmentNo, destinationOffset += copyCount)
        {
            copyCount = Math.Min(fragmentsOfByte[currentFragmentNo].Memory.Length, Length - destinationOffset);
            await destination.WriteAsync(fragmentsOfByte[currentFragmentNo].Memory[..copyCount], cancellationToken);
        }

        // If destinationOffset > copyCount we copied from more than one fragment, so the currentPosition would simply be the copyCount.
        // Otherwise, we should add startingPosition.Offset to the copyCount, because we copied from the first fragment only,
        // and copying from the 1st one does not necessarily starts from its beginning.
        // Without branching: var currentPosition = (destinationOffset > copyCount ? 0 : startingPosition.Offset) + copyCount
        var currentPosition = (startingPosition.Offset & (((copyCount - destinationOffset) >>> 31) - 1)) + copyCount;

        // If the last copied fragment's length is equal to the currentPosition, then we copied the whole fragment, thus the next position is the 0th offset of the next fragment.
        return _fragments[currentFragmentNo - 1].Memory.Length != currentPosition
            ? new(currentFragmentNo - 1, currentPosition)
            : new(currentFragmentNo, 0);

    EndOfStream:
        return EndOfStreamPosition;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by a managed pointer.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedPosition"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal FragmentedPosition CopyTo(ref T destination)
    {
        var startingPosition = _fragmentedPosition != FragmentedPosition.NotFound
            ? _fragmentedPosition
            : GetFragmentedPosition(_offset);

        if (startingPosition == FragmentedPosition.NotFound || startingPosition == EndOfStreamPosition)
        {
            goto EndOfStream;
        }

        // Copy from first fragment: it needs special handling because it doesn't necessarily start from 0.
        var firstFragmentMemory = _fragments[startingPosition.FragmentNo].Memory[startingPosition.Offset..];
        int copyCount = Math.Min(firstFragmentMemory.Length, Length);

        Ptr.UnalignedCopy(ref destination, ref Ptr.Get(firstFragmentMemory.Span), copyCount);

        int currentFragmentNo = startingPosition.FragmentNo + 1;
        ref T pDestination = ref Unsafe.Add(ref destination, copyCount);
        ref T boundary = ref Unsafe.Add(ref destination, Length);

        // Copy from subsequent fragments if needed.
        for (;
            Unsafe.IsAddressLessThan(ref pDestination, ref boundary);
            ++currentFragmentNo, pDestination = ref Unsafe.Add(ref pDestination, copyCount))
        {
            copyCount = Math.Min(_fragments[currentFragmentNo].Memory.Length, (int)Unsafe.ByteOffset(ref pDestination, ref boundary));
            Ptr.UnalignedCopy(ref pDestination, ref Ptr.Get(_fragments[currentFragmentNo].Memory.Span[..copyCount]), copyCount);
        }

        // If destinationOffset > copyCount we copied from more than one fragment, so the currentPosition would simply be the copyCount.
        // Otherwise, we should add startingPosition.Offset to the copyCount, because we copied from the first fragment only,
        // and copying from the 1st one does not necessarily starts from its beginning.
        // Without branching: var currentPosition = (destinationOffset > copyCount ? 0 : startingPosition.Offset) + copyCount
        var currentPosition = (startingPosition.Offset & (((copyCount - (int)Unsafe.ByteOffset(ref destination, ref pDestination)) >>> 31) - 1)) + copyCount;

        // If the last copied fragment's length is equal to the currentPosition, then we copied the whole fragment, thus the next position is the 0th offset of the next fragment.
        return _fragments[currentFragmentNo - 1].Memory.Length != currentPosition
            ? new(currentFragmentNo - 1, currentPosition)
            : new(currentFragmentNo, 0);

    EndOfStream:
        return EndOfStreamPosition;
    }

    /// <summary>
    /// Gets the <see cref="FragmentedPosition"/> of a specific offset.
    /// </summary>
    /// <param name="offset">The offset to find the <see cref="FragmentedPosition"/> for.</param>
    /// <returns>A <see cref="FragmentedPosition"/> containing the fragmentNumber and the offset within the fragment.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Only implemented in a separate method for readability. Longer/redundant code is better than emitting a call here.
    private FragmentedPosition GetFragmentedPosition(int offset)
    {
        ref var fragment = ref Ptr.Get(_fragments);

        int lowerBoundary = 0;
        int upperBoundary = _fragmentCount - 1;
        int fragmentNoToCheck;

        // Binary search _fragments for the offset.
        while (lowerBoundary <= upperBoundary)
        {
            fragmentNoToCheck = (lowerBoundary + upperBoundary) >> 1;
            var fragmentToCheck = Unsafe.Add(ref fragment, fragmentNoToCheck);

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

    #endregion
}
