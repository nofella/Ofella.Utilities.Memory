namespace Ofella.Utilities.Memory.Defragmentation;

/// <summary>
/// Represents a two-dimensional position specific to fragmented memories.
/// </summary>
/// <param name="FragmentNo">The ordinal of the fragment in which the <paramref name="OffsetFromFragment"/> should be interpreted.</param>
/// <param name="OffsetFromFragment">The offset inside the fragment at <paramref name="FragmentNo"/>.</param>
public readonly record struct FragmentedMemoryEnumerator
{
    /// <summary>
    /// Represents a not found, non existing or invalid position.
    /// </summary>
    public static readonly FragmentedMemoryEnumerator None = new(-1, -1);

    /// <summary>
    /// Represents the beginning of a fragmented memory.
    /// </summary>
    public static readonly FragmentedMemoryEnumerator Beginning = new(0, 0);

    public readonly int FragmentNo;
    public readonly int OffsetFromFragment;

    internal FragmentedMemoryEnumerator(int fragmentNo, int offsetFromFragment)
    {
        this.FragmentNo = fragmentNo;
        this.OffsetFromFragment = offsetFromFragment;
    }
}
