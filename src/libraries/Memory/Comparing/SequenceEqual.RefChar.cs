using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Ofella.Utilities.Memory.ManagedPointers;
using Ofella.Utilities.Memory.Vectors;

namespace Ofella.Utilities.Memory.Comparing;

public static partial class Comparer
{
    /// <summary>
    /// SequenceEqual for strings having a length of 2 or 3.
    /// This implementation enforces the aforementioned length limitation.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualShort(ref char left, ref char right, nuint length)
    {
        if ((length & 0xFFFFFFFFFFFFFFFE) != 2) // will be 2 for the following values only: 2 and 3
        {
            throw new ArgumentOutOfRangeException(nameof(length)); // Branch prediction optimization: JIT creates forward jump which is predicted not taken.
        }

        return SequenceEqualShortUnsafe(ref left, ref right, length);
    }

    /// <summary>
    /// SequenceEqual for strings having a length of 2 or 3.
    /// This implementation does not enforce the aforementioned length limitation.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualShortUnsafe(ref char left, ref char right, nuint length)
    {
        nuint secondOffset = length - 2;
        uint firstPart = left.AsDwordPtr() - right.AsDwordPtr();
        uint secondPart = left.AsDwordPtr(secondOffset) - right.AsDwordPtr(secondOffset);
        return (firstPart | secondPart) == 0;
    }

    /// <summary>
    /// SequenceEqual for strings having a length of: 4,5,6 or 7.
    /// This implementation enforces the aforementioned length limitation.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualMedium(ref char left, ref char right, nuint length)
    {
        if ((length & 0xFFFFFFFFFFFFFFFC) != 4) // will be 2 for the following values only: 4,5,6 and 7
        {
            throw new ArgumentOutOfRangeException(nameof(length)); // Branch prediction optimization: JIT creates forward jump which is predicted not taken.
        }

        return SequenceEqualMediumUnsafe(ref left, ref right, length);
    }

    /// <summary>
    /// SequenceEqual for strings having a length of: 4,5,6 or 7.
    /// This implementation does not enforce the aforementioned length limitation.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualMediumUnsafe(ref char left, ref char right, nuint length)
    {
        nuint secondOffset = length - 4;
        ulong firstPart = left.AsQwordPtr() - right.AsQwordPtr();
        ulong secondPart = left.AsQwordPtr(secondOffset) - right.AsQwordPtr(secondOffset);
        return (firstPart | secondPart) == 0;
    }

    /// <summary>
    /// SequenceEqual for strings having a length greater than 7.
    /// This implementation enforces the aforementioned length limitation.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualLong(ref char left, ref char right, nuint length)
    {
        if (length < 8)
        {
            throw new ArgumentOutOfRangeException(nameof(length)); // Branch prediction optimization: JIT creates forward jump which is predicted not taken.
        }

        return SequenceEqualLongUnsafe(ref left, ref right, length);
    }

    /// <summary>
    /// SequenceEqual for strings having a length greater than 7.
    /// This implementation does not enforce the aforementioned length limitation.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualLongUnsafe(ref char left, ref char right, nuint length)
    {
        // Taking advantage of string interning
        if (Unsafe.AreSame(ref left, ref right))
            goto ReferenceEqual; // Branch prediction optimization: JIT creates forward jump which is predicted not taken.

        // Vectors are not supported: compare using qwords.
        if (!Vector128.IsHardwareAccelerated)
            goto VectorIsNotSupported; // No need for branch prediciton optimization, since the IsHardwareAccelerated property is a "JIT time" constant.

        // Can be vectorized using Vector128
        if (!Vector256.IsHardwareAccelerated || length < (nuint)Vector256<ushort>.Count)
        {
            nuint offset = 0;
            nuint lastVectorStart = length - (nuint)Vector128<ushort>.Count;

            if (lastVectorStart != 0)
            {
                do
                {
                    if (Vector128Extensions.CreateFromChar(ref left, offset) != Vector128Extensions.CreateFromChar(ref right, offset))
                    {
                        return false;
                    }

                    offset += (nuint)Vector128<ushort>.Count;
                }
                while (offset < lastVectorStart);
            }

            return Vector128Extensions.CreateFromChar(ref left, lastVectorStart) == Vector128Extensions.CreateFromChar(ref right, lastVectorStart);
        }

        // Can be vectorized using Vector256
        {
            nuint offset = 0;
            nuint lastVectorStart = length - (nuint)Vector256<ushort>.Count;

            if (lastVectorStart != 0)
            {
                do
                {
                    if (Vector256Extensions.CreateFromChar(ref left, offset) != Vector256Extensions.CreateFromChar(ref right, offset))
                    {
                        return false;
                    }

                    offset += (nuint)Vector256<ushort>.Count;
                }
                while (offset < lastVectorStart);
            }

            return Vector256Extensions.CreateFromChar(ref left, lastVectorStart) == Vector256Extensions.CreateFromChar(ref right, lastVectorStart);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualWithMoreBranches(ref char left, ref char right, nuint length)
    {
        // Compare as WORD
        if (length == 1)
            goto SingleCharacterString; // Branch prediction optimization: JIT creates forward jump which is predicted not taken.

        // Unroll 2 DWORD compare
        if (length < 4)
            goto ShorterThan4Chars; // Branch prediction optimization: JIT creates forward jump which is predicted not taken.

        // Unroll 2 QWORD compare
        if (length < 8)
            goto ShorterThan8Chars; // Branch prediction optimization: JIT creates forward jump which is predicted not taken.

        return SequenceEqualLongUnsafe(ref left, ref right, length);

    SingleCharacterString:
        return left == right;

    ShorterThan4Chars:
        return SequenceEqualShortUnsafe(ref left, ref right, length);

    ShorterThan8Chars:
        return SequenceEqualMediumUnsafe(ref left, ref right, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualWithJumpTable(ref char left, ref char right, nuint length)
    {
        // Helping JIT to create a jump table
        switch (length)
        {
            case 1:
                return left == right;
            case 2:
                return left.AsDwordPtr() == right.AsDwordPtr();
            case 3:
                return left.AsDwordPtr() == right.AsDwordPtr() && left.AsDwordPtr(1) == right.AsDwordPtr(1);
            case 4:
                return left.AsQwordPtr() == right.AsQwordPtr();
            case 5:
                return left.AsQwordPtr() == right.AsQwordPtr() && left.AsQwordPtr(1) == right.AsQwordPtr(1);
            case 6:
                return left.AsQwordPtr() == right.AsQwordPtr() && left.AsQwordPtr(2) == right.AsQwordPtr(2);
            case 7:
                return left.AsQwordPtr() == right.AsQwordPtr() && left.AsQwordPtr(3) == right.AsQwordPtr(3);
        }

        return SequenceEqualLongUnsafe(ref left, ref right, length);
    }
}
