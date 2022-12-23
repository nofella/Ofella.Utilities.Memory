using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ofella.Utilities.Memory.Comparing;

public static partial class ReadOnlySpanOfCharExtensions
{
    public static bool EqualsOrdinal(this string s1, string s2)
    {
        return EqualsOrdinal(s1.AsSpan(), s2.AsSpan());
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool EqualsOrdinal(this ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        // Pointless to check equality if lengths are not matched
        if (left.Length != right.Length) return false;

        ref var pLeft = ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(left));
        ref var pRight = ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(right));


        return EqualityComparer.Equals(
            ref pLeft,
            ref pRight,
            (nuint)left.Length);
    }
}
