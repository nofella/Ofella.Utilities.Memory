using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ofella.Utilities.Memory.Comparing;

public static partial class Comparer
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqual(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        // Pointless to check equality if lengths are not matched
        if (left.Length != right.Length) return false;

        return SequenceEqual(
            ref MemoryMarshal.GetReference(left),
            ref MemoryMarshal.GetReference(right),
            (nuint)left.Length);
    }
}
