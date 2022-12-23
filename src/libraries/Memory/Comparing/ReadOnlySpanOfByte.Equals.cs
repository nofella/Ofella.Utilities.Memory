using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ofella.Utilities.Memory.Comparing;

public static partial class ReadOnlySpanOfByteExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Equals(this ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        // Pointless to check equality if lengths are not matched
        if (left.Length != right.Length) return false;

        return EqualityComparer.Equals(
            ref MemoryMarshal.GetReference(left),
            ref MemoryMarshal.GetReference(right),
            (nuint)left.Length);
    }
}
