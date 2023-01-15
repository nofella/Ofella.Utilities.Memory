namespace Ofella.Utilities.Memory.Defragmentation;

public readonly record struct FragmentedPosition(int FragmentNo, int Offset)
{
    public static readonly FragmentedPosition NotFound = new(-1, -1);

    public static readonly FragmentedPosition Beginning = new(0, 0);
}
