using System.Runtime.CompilerServices;

namespace Ofella.Utilities.Memory.ManagedPointers;

public static partial class ReinterpretCastExtensions
{
    #region BYTE to BYTE

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref byte AsBytePtr(this ref byte ptr) => ref AsBytePtr(ref ptr, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref byte AsBytePtr(this ref byte ptr, int offset)
    {
        return ref Unsafe.Add(ref ptr, offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref byte AsBytePtr(this ref byte ptr, nuint offset)
    {
        return ref Unsafe.Add(ref ptr, offset);
    }

    #endregion

    #region BYTE to WORD

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ushort AsWordPtr(this ref byte ptr) => ref AsWordPtr(ref ptr, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ushort AsWordPtr(this ref byte ptr, int offset)
    {
        return ref Unsafe.As<byte, ushort>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ushort AsWordPtr(this ref byte ptr, nuint offset)
    {
        return ref Unsafe.As<byte, ushort>(ref Unsafe.Add(ref ptr, offset));
    }

    #endregion

    #region BYTE to DWORD

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref uint AsDwordPtr(this ref byte ptr) => ref AsDwordPtr(ref ptr, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref uint AsDwordPtr(this ref byte ptr, int offset)
    {
        return ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref uint AsDwordPtr(this ref byte ptr, nuint offset)
    {
        return ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref ptr, offset));
    }

    #endregion

    #region BYTE to QWORD

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ulong AsQwordPtr(this ref byte ptr) => ref AsQwordPtr(ref ptr, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ulong AsQwordPtr(this ref byte ptr, int offset)
    {
        return ref Unsafe.As<byte, ulong>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ulong AsQwordPtr(this ref byte ptr, nuint offset)
    {
        return ref Unsafe.As<byte, ulong>(ref Unsafe.Add(ref ptr, offset));
    }

    #endregion
}
