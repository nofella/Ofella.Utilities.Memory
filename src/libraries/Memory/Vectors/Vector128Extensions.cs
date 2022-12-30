using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Ofella.Utilities.Memory.Vectors;

public static class Vector128Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector128<ushort> CreateFromChar(ref char pChar) => Vector128.LoadUnsafe(ref Unsafe.As<char, ushort>(ref pChar));

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector128<ushort> CreateFromChar(ref char pChar, nuint elementOffset) => Vector128.LoadUnsafe(ref Unsafe.As<char, ushort>(ref pChar), elementOffset);
}
