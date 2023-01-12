using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ofella.Utilities.Memory.Defragmentation;

/// <summary>
/// Abstracts multiple chunks of <see cref="Memory{T}"/> as a single contiguous region, without copying them anywhere.
/// </summary>
/// <typeparam name="T">The type of the elements in the <see cref="FragmentedMemory{T}"/>.</typeparam>
public readonly struct FragmentedMemory<T>
{
    private readonly MemoryFragment<T>[] _fragments;
    private readonly int _offset;

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
        _fragments = new MemoryFragment<T>[fragments.Length];
        _offset = 0;

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
        _offset = offset;
        Length = length;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FragmentedMemory<T> Slice(int offset, int length) => new(in this, _offset + offset, length);

    public FragmentedMemory<T> this[Range range] => Slice(_offset + range.Start.Value, range.End.Value - range.Start.Value);


    public void CopyTo(Memory<T> destination)
    {
        CopyTo((memory, destinationOffset) =>
        {
            memory.CopyTo(destination[destinationOffset..]);
        });
    }

    public void CopyTo(T[] destination)
    {
        CopyTo((memory, destinationOffset) =>
        {
            memory.CopyTo(destination.AsMemory()[destinationOffset..]);
        });
    }

    internal void CopyTo(Action<Memory<T>, int> copyAction)
    {
        // Optimized for sequential copy from start: avoid the binary search if the _offset is 0.
        var firstFragment = _offset == 0 ? (fragmentNo: 0, offset: 0) : GetFragmentAt(_offset);

        if (firstFragment.fragmentNo == -1)
        {
            goto EndOfStream;
        }

        for (int i = firstFragment.fragmentNo, destinationOffset = 0;
            destinationOffset < Length; ++i)
        {
            int bytesToCopy = Math.Min(_fragments[i].Memory.Length, Length - destinationOffset);
            copyAction(_fragments[i].Memory[..bytesToCopy], destinationOffset);
            destinationOffset += bytesToCopy;
        }

    EndOfStream:
        return;
    }

    internal async ValueTask CopyToAsync(Func<Memory<T>, long, CancellationToken, ValueTask> copyAction, CancellationToken cancellationToken = default)
    {
        // Optimized for sequential copy from start: avoid the binary search if the _offset is 0.
        var firstFragment = _offset == 0 ? (fragmentNo: 0, offset: 0) : GetFragmentAt(_offset);

        // For cleaner code: separated copy logic for cases when the total length to be copied is smaller than the first segment 
        if (Length < (uint)_fragments[0].Memory.Length)
        {
            await copyAction(_fragments[0].Memory[(int)firstFragment.offset..(int)Length], 0, cancellationToken);
            return;
        }

        int destinationOffset = 0;

        for (int i = firstFragment.fragmentNo; destinationOffset < Length; ++i)
        {
            int bytesToCopy = Math.Min(_fragments[i].Memory.Length, (int)Length - destinationOffset);
            await copyAction(_fragments[i].Memory[..bytesToCopy], destinationOffset, cancellationToken);
            destinationOffset += bytesToCopy;
        }
    }

    /// <summary>
    /// Optimized for moving to a specific offset using binary search. This method has a performance of O(log n).
    /// </summary>
    /// <param name="offset">The offset to move to.</param>
    private (int fragmentNo, int offset) GetFragmentAt(int offset)
    {
        ref var fragmentsPtr = ref MemoryMarshal.GetArrayDataReference(_fragments);

        int lowerBoundary = 0;
        int upperBoundary = _fragments.Length - 1;
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
                return (fragmentNoToCheck, offset - _fragments[fragmentNoToCheck].Offset);
            }
        }

        return (-1, -1); // Not found.
    }
}
