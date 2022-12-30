using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Ofella.Utilities.Memory.ManagedPointers;

namespace Ofella.Utilities.Memory.Comparing;

public static partial class Comparer
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqual(ref byte left, ref byte right, nuint length)
    {
        if (length < 16)
            goto Shorter; // Branch prediction optimization: JIT creates forward jump which is predicted not taken.

        // Taking advantage of string interning
        if (Unsafe.AreSame(ref left, ref right))
            goto ReferenceEqual; // Branch prediction optimization: JIT creates forward jump which is predicted not taken.

        // Vectors are not supported: compare using qwords.
        if (!Vector128.IsHardwareAccelerated)
            goto VectorIsNotSupported; // No need for branch prediciton optimization, since the IsHardwareAccelerated property is a "JIT time" constant.

        // Can be vectorized using Vector128
        if (!Vector256.IsHardwareAccelerated || length < (nuint)Vector256<byte>.Count)
        {
            nuint offset = 0;
            nuint lastVectorStart = length - (nuint)Vector128<byte>.Count;

            if (lastVectorStart != 0)
            {
                do
                {
                    if (Vector128.Xor(Vector128.LoadUnsafe(ref left, offset), Vector128.LoadUnsafe(ref right, offset)) != Vector128<byte>.Zero)
                    {
                        return false;
                    }

                    offset += (nuint)Vector128<byte>.Count;
                }
                while (offset < lastVectorStart);
            }

            return Vector128.Xor(Vector128.LoadUnsafe(ref left, lastVectorStart), Vector128.LoadUnsafe(ref right, lastVectorStart)) == Vector128<byte>.Zero;
        }

        // Can be vectorized using Vector256
        {
            nuint offset = 0;
            nuint lastVectorStart = length - (nuint)Vector256<byte>.Count;

            if (lastVectorStart != 0)
            {
                do
                {
                    if (Vector256.Xor(Vector256.LoadUnsafe(ref left, offset), Vector256.LoadUnsafe(ref right, offset)) != Vector256<byte>.Zero)
                    {
                        return false;
                    }

                    offset += (nuint)Vector256<byte>.Count;
                }
                while (offset < lastVectorStart);
            }

            return Vector256.Xor(Vector256.LoadUnsafe(ref left, lastVectorStart), Vector256.LoadUnsafe(ref right, lastVectorStart)) == Vector256<byte>.Zero;
        }

    Shorter:
        // Helping JIT to create a jump table
        switch ((uint)length) // JIT double-checks length when not converted to uint (no idea why)
        {
            case 1:
                return left == right;
            case 2:
                return left.AsWordPtr() == right.AsWordPtr();
            case 3:
                return left.AsWordPtr() == right.AsWordPtr() && left.AsWordPtr(1) == right.AsWordPtr(1);
            case 4:
                return left.AsDwordPtr() == right.AsDwordPtr();
            case 5:
                return left.AsDwordPtr() == right.AsDwordPtr() && left.AsDwordPtr(1) == right.AsDwordPtr(1);
            case 6:
                return left.AsDwordPtr() == right.AsDwordPtr() && left.AsDwordPtr(2) == right.AsDwordPtr(2);
            case 7:
                return left.AsDwordPtr() == right.AsDwordPtr() && left.AsDwordPtr(3) == right.AsDwordPtr(3);
            case 8:
                return left.AsQwordPtr() == right.AsQwordPtr();
            case 9:
                return left.AsQwordPtr() == right.AsQwordPtr() && left.AsQwordPtr(1) == right.AsQwordPtr(1);
            case 10:
                return left.AsQwordPtr() == right.AsQwordPtr() && left.AsQwordPtr(2) == right.AsQwordPtr(2);
            case 11:
                return left.AsQwordPtr() == right.AsQwordPtr() && left.AsQwordPtr(3) == right.AsQwordPtr(3);
            case 12:
                return left.AsQwordPtr() == right.AsQwordPtr() && left.AsQwordPtr(4) == right.AsQwordPtr(4);
            case 13:
                return left.AsQwordPtr() == right.AsQwordPtr() && left.AsQwordPtr(5) == right.AsQwordPtr(5);
            case 14:
                return left.AsQwordPtr() == right.AsQwordPtr() && left.AsQwordPtr(6) == right.AsQwordPtr(6);
            case 15:
                return left.AsQwordPtr() == right.AsQwordPtr() && left.AsQwordPtr(7) == right.AsQwordPtr(7);
        }

    ReferenceEqual:
        return true;

    VectorIsNotSupported:
        {
            nuint offset = 0;
            nuint lastQwordStart = length - 4;

            do
            {
                if (left.AsQwordPtr(offset) != right.AsQwordPtr(offset))
                {
                    return false;
                }

                offset += 4;
            } while (offset < lastQwordStart);

            return left.AsQwordPtr(lastQwordStart) == right.AsQwordPtr(lastQwordStart);
        }
    }
}
