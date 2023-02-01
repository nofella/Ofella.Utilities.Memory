namespace Ofella.Utilities.Memory.Defragmentation;

/// <summary>
/// Represents a two-dimensional position specific to fragmented memories.
/// </summary>
/// <param name="FragmentNo">The ordinal of the fragment in which the <paramref name="Offset"/> should be interpreted.</param>
/// <param name="Offset">The offset inside the fragment at <paramref name="FragmentNo"/>.</param>
public readonly record struct FragmentedPosition(int FragmentNo, int Offset)
{
    /// <summary>
    /// Represents a not found, non existing or invalid position.
    /// </summary>
    public static readonly FragmentedPosition NotFound = new(-1, -1);

    /// <summary>
    /// Represents the beginning of a fragmented memory.
    /// </summary>
    public static readonly FragmentedPosition Beginning = new(0, 0);
}
