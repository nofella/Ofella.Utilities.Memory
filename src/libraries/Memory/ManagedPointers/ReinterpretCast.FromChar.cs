using System.Runtime.CompilerServices;

namespace Ofella.Utilities.Memory.ManagedPointers;

public static partial class ReinterpretCastExtensions
{
    #region WORD to BYTE

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref byte AsBytePtr(this ref char ptr) => ref AsBytePtr(ref ptr, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref byte AsBytePtr(this ref char ptr, int offset)
    {
        return ref Unsafe.As<char, byte>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref byte AsBytePtr(this ref char ptr, nuint offset)
    {
        return ref Unsafe.As<char, byte>(ref Unsafe.Add(ref ptr, offset));
    }

    #endregion

    #region WORD to DWORD

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref uint AsDwordPtr(this ref char ptr) => ref AsDwordPtr(ref ptr, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref uint AsDwordPtr(this ref char ptr, int offset)
    {
        return ref Unsafe.As<char, uint>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref uint AsDwordPtr(this ref char ptr, nuint offset)
    {
        return ref Unsafe.As<char, uint>(ref Unsafe.Add(ref ptr, offset));
    }

    #endregion

    #region WORD to QWORD

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ulong AsQwordPtr(this ref char ptr) => ref AsQwordPtr(ref ptr, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ulong AsQwordPtr(this ref char ptr, int offset)
    {
        return ref Unsafe.As<char, ulong>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ulong AsQwordPtr(this ref char ptr, nuint offset)
    {
        return ref Unsafe.As<char, ulong>(ref Unsafe.Add(ref ptr, offset));
    }

    #endregion
}
