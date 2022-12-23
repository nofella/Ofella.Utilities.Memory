using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Ofella.Utilities.Memory.ManagedPointers;

namespace Ofella.Utilities.Memory.Comparing;

public static partial class EqualityComparer
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Equals(this ref byte left, ref byte right, nuint length)
    {
        // Cannot be vectorized due to small length, or lack of hw support for vectors
        if (!Vector128.IsHardwareAccelerated || length < (nuint)Vector128<ushort>.Count)
        {
            switch (length)
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

                // no hw support: "fake" vectorizing using ulong
                default:
                    // Taking advantage of string interning
                    if (Unsafe.AreSame(ref left, ref right)) return true;

                    for (nuint i = 0; i < length; ++i)
                    {
                        // TODO: Use int64 compare, and compare the remained using smaller int types. Loop can be safely unrolled because length is at least 8
                        if (Unsafe.Add(ref left, i) != Unsafe.Add(ref right, i))
                        {
                            return false;
                        }
                    }

                    return true;

            }
        }

        // Taking advantage of string interning
        if (Unsafe.AreSame(ref left, ref right)) return true;

        // Can be vectorized using Vector128
        if (!Vector256.IsHardwareAccelerated || length < (nuint)Vector256<ushort>.Count)
        {
            nuint offset = 0;
            nuint lastVectorStart = length - (nuint)Vector128<ushort>.Count;

            while (offset < lastVectorStart)
            {
                if (Vector128.Xor(Vector128.LoadUnsafe(ref left, offset), Vector128.LoadUnsafe(ref right, offset)) != Vector128<byte>.Zero)
                {
                    return false;
                }

                offset += (nuint)Vector128<ushort>.Count;
            }

            return Vector128.Xor(Vector128.LoadUnsafe(ref left, offset), Vector128.LoadUnsafe(ref right, offset)) == Vector128<byte>.Zero;
        }

        // Can be vectorized using Vector256
        {
            nuint offset = 0;
            nuint lastVectorStart = length - (nuint)Vector256<ushort>.Count;

            while (offset < lastVectorStart)
            {
                if (Vector256.Xor(Vector256.LoadUnsafe(ref left, offset), Vector256.LoadUnsafe(ref right, offset)) != Vector256<byte>.Zero)
                {
                    return false;
                }

                offset += (nuint)Vector256<ushort>.Count;
            }

            return Vector256.Xor(Vector256.LoadUnsafe(ref left, offset), Vector256.LoadUnsafe(ref right, offset)) == Vector256<byte>.Zero;
        }
    }
}
