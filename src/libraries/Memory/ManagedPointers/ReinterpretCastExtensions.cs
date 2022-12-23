using System.Runtime.CompilerServices;

namespace Ofella.Utilities.Memory.ManagedPointers;

public static class ReinterpretCastExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ushort AsWordPtr(this ref byte ptr) => ref AsWordPtr(ref ptr, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref uint AsDwordPtr(this ref byte ptr) => ref AsDwordPtr(ref ptr, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ulong AsQwordPtr(this ref byte ptr) => ref AsQwordPtr(ref ptr, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ushort AsWordPtr(this ref byte ptr, int offset)
    {
        return ref Unsafe.As<byte, ushort>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref uint AsDwordPtr(this ref byte ptr, int offset)
    {
        return ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ulong AsQwordPtr(this ref byte ptr, int offset)
    {
        return ref Unsafe.As<byte, ulong>(ref Unsafe.Add(ref ptr, offset));
    }
}
