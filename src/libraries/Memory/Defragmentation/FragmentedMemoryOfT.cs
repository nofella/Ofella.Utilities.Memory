namespace Ofella.Utilities.Memory.Defragmentation;

public readonly struct FragmentedMemory<T>
{
    private readonly MemoryFragment<T>[] _fragments;
    private readonly long _offset;

    public long Length { get; init; }

    public FragmentedMemory(Memory<T>[] fragments)
    {
        _fragments = new MemoryFragment<T>[fragments.Length];
        _offset = 0;

        int calculatedOffset = 0;

        for (var i = 0; i < fragments.Length; i++)
        {
            _fragments[i] = new(fragments[i], calculatedOffset);
            calculatedOffset += fragments[i].Length;
        }

        Length = calculatedOffset;
    }

    public FragmentedMemory(in FragmentedMemory<T> fragmentedMemory, long offset, long length)
    {
        _fragments = fragmentedMemory._fragments;
        _offset = offset;
        Length = length;
    }


    public FragmentedMemory<T> Slice(long offset, long length) => new(in this, _offset + offset, length);

    public FragmentedMemory<T> this[Range range] => new(in this, _offset + range.Start.Value, range.End.Value - range.Start.Value);


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

        // For cleaner code: separated copy logic for cases when the total length to be copied is smaller than the first segment 
        if (Length < _fragments[0].Memory.Length)
        {
            copyAction(_fragments[0].Memory[(int)firstFragment.offset..(int)Length], 0);
            return;
        }

        int destinationOffset = 0;

        for (int i = firstFragment.fragmentNo; destinationOffset < Length; ++i)
        {
            int bytesToCopy = Math.Min(_fragments[i].Memory.Length, (int)Length - destinationOffset);
            copyAction(_fragments[i].Memory[..bytesToCopy], destinationOffset);
            destinationOffset += bytesToCopy;
        }
    }

    internal async ValueTask CopyToAsync(Func<Memory<T>, int, CancellationToken, ValueTask> copyAction, CancellationToken cancellationToken = default)
    {
        // Optimized for sequential copy from start: avoid the binary search if the _offset is 0.
        var firstFragment = _offset == 0 ? (fragmentNo: 0, offset: 0) : GetFragmentAt(_offset);

        // For cleaner code: separated copy logic for cases when the total length to be copied is smaller than the first segment 
        if (Length < _fragments[0].Memory.Length)
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
    /// Optimized for moving to a specific offset. This method has a performance of O(log n).
    /// </summary>
    /// <param name="offset">The offset to move to.</param>
    private (int fragmentNo, long offset) GetFragmentAt(long offset)
    {
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        if (offset >= Length)
        {
            return (_fragments.Length - 1, _fragments[^1].Memory.Length - 1);
        }

        var lowerBoundary = 0;
        var upperBoundary = _fragments.Length - 1;

        while (lowerBoundary <= upperBoundary)
        {
            var fragmentToCheck = (int)Math.Floor((decimal)(lowerBoundary + upperBoundary) / 2);
            var direction = GetDirection(fragmentToCheck, offset);

            if (direction < 0)
            {
                upperBoundary = fragmentToCheck - 1;
            }
            else if (direction > 0)
            {
                lowerBoundary = fragmentToCheck + 1;
            }
            else
            {
                return (fragmentToCheck, offset - GetFragmentOffset(fragmentToCheck));
            }
        }

        // Unreachable code by logic
        throw new InvalidOperationException();
    }

    private int GetDirection(int fragmentNo, long position)
    {
        if (position < GetFragmentOffset(fragmentNo))
        {
            return -1;
        }
        else if (position >= GetFragmentOffset(fragmentNo + 1))
        {
            return 1;
        }

        return 0;
    }

    private long GetFragmentOffset(int fragmentNo)
    {
        if (fragmentNo < 0)
        {
            return 0;
        }

        if (fragmentNo >= _fragments.Length)
        {
            return Length;
        }

        return _fragments[fragmentNo].Offset;
    }
}
