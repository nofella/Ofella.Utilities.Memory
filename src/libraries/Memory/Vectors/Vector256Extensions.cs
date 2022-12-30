using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Ofella.Utilities.Memory.Vectors;

public static class Vector256Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector256<ushort> CreateFromChar(ref char pChar) => Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref pChar));

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector256<ushort> CreateFromChar(ref char pChar, nuint elementOffset) => Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref pChar), elementOffset);
}
