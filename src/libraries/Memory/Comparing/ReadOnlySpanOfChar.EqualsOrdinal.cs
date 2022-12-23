using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Ofella.Utilities.Memory.Comparing
{
    public static partial class ReadOnlySpanOfCharExtensions
    {
        public static bool EqualsOrdinal(this string s1, string s2)
        {
            return EqualsOrdinal(s1.AsSpan(), s2.AsSpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsOrdinal(this ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
        {
            // Pointless to check equality if lengths are not matched
            if (s1.Length != s2.Length) return false;

            nuint length = (nuint)s1.Length;

            ref var p1int16 = ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(s1));
            ref var p2int16 = ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(s2));

            if (!Vector128.IsHardwareAccelerated || length < (nuint)Vector128<ushort>.Count)
            {
                // Simple integer comparison with unrolling up to 7 chars
                // 8 chars comparison is not unrolled, since vectorization is faster in that case
                switch (length)
                {
                    case 7: // compare as one qword, one dword and one word
                        return Unsafe.As<ushort, ulong>(ref p1int16) == Unsafe.As<ushort, ulong>(ref p2int16)
                            && Unsafe.As<ushort, uint>(ref Unsafe.Add(ref p1int16, 4)) == Unsafe.As<ushort, uint>(ref Unsafe.Add(ref p2int16, 4))
                            && Unsafe.Add(ref p1int16, 6) == Unsafe.Add(ref p2int16, 6);

                    case 6: // compare as one qword and one dword
                        return Unsafe.As<ushort, ulong>(ref p1int16) == Unsafe.As<ushort, ulong>(ref p2int16)
                            && Unsafe.As<ushort, uint>(ref Unsafe.Add(ref p1int16, 4)) == Unsafe.As<ushort, uint>(ref Unsafe.Add(ref p2int16, 4));

                    case 5: // compare as one qword and one word
                        return Unsafe.As<ushort, ulong>(ref p1int16) == Unsafe.As<ushort, ulong>(ref p2int16)
                            && Unsafe.Add(ref p1int16, 4) == Unsafe.Add(ref p2int16, 4);

                    case 4: // compare as qwords
                        return Unsafe.As<ushort, ulong>(ref p1int16) == Unsafe.As<ushort, ulong>(ref p2int16);

                    case 3: // compare as one dword and one word as a qword
                        return (ulong)Unsafe.As<ushort, uint>(ref p1int16) * 4 + Unsafe.Add(ref p1int16, 2) == (ulong)Unsafe.As<ushort, uint>(ref p2int16) * 4 + Unsafe.Add(ref p2int16, 2);

                    case 2: // compare as dwords
                        return Unsafe.As<ushort, uint>(ref p1int16) - Unsafe.As<ushort, uint>(ref p2int16) == 0;

                    case 1: // compare as words
                        return p1int16 == p2int16;

                    default: // length >= 8
                             // Taking advantage of string interning mainly
                        if (Unsafe.AreSame(ref MemoryMarshal.GetReference(s1), ref MemoryMarshal.GetReference(s2))) return true;

                        for (nuint i = 0; i < length; ++i)
                        {
                            // TODO: Use int64 compare, and compare the remained using smaller int types. Loop can be safely unrolled because length is at least 8
                            if (Unsafe.Add(ref p1int16, i) != Unsafe.Add(ref p2int16, i))
                            {
                                return false;
                            }
                        }

                        return true;

                }
            }

            // Taking advantage of string interning mainly
            if (Unsafe.AreSame(ref MemoryMarshal.GetReference(s1), ref MemoryMarshal.GetReference(s2))) return true;

            // Can be vectorized using Vector128
            if (!Vector256.IsHardwareAccelerated || length < (nuint)Vector256<ushort>.Count)
            {
                nuint offset = 0;
                nuint lastVectorStart = length - (nuint)Vector128<ushort>.Count;

                while (offset < lastVectorStart)
                {
                    if (Vector128.Xor(Vector128.LoadUnsafe(ref p1int16, offset), Vector128.LoadUnsafe(ref p2int16, offset)) != Vector128<ushort>.Zero)
                    {
                        return false;
                    }

                    offset += (nuint)Vector128<ushort>.Count;
                }

                return Vector128.Xor(Vector128.LoadUnsafe(ref p1int16, offset), Vector128.LoadUnsafe(ref p2int16, offset)) == Vector128<ushort>.Zero;
            }

            // Can be vectorized using Vector256
            {
                nuint offset = 0;
                nuint lastVectorStart = length - (nuint)Vector256<ushort>.Count;

                while (offset < lastVectorStart)
                {
                    if (Vector256.Xor(Vector256.LoadUnsafe(ref p1int16, offset), Vector256.LoadUnsafe(ref p2int16, offset)) != Vector256<ushort>.Zero)
                    {
                        return false;
                    }

                    offset += (nuint)Vector256<ushort>.Count;
                }

                return Vector256.Xor(Vector256.LoadUnsafe(ref p1int16, offset), Vector256.LoadUnsafe(ref p2int16, offset)) == Vector256<ushort>.Zero;
            }
        }
    }
}
