using Ofella.Utilities.Memory.Defragmentation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace Ofella.Utilities.Memory.Tests.Defragmentation;

public abstract class BaseTest
{
    private static readonly int[] FragmentAndReadSizes = new int[] { 1, 10, 100, 1_000, 10_000, 100_000, 32, 64, 128, 512, 950, 1024, 4096, 33_333, 64_112, 150_000 };

    private static byte[] ByteArray100k;
    private static sbyte[] SByteArray100k;
    private static ushort[] UShortArray100k;
    private static short[] ShortArray100k;
    private static uint[] UIntArray100k;
    private static int[] IntArray100k;
    private static ulong[] ULongArray100k;
    private static long[] LongArray100k;

    static BaseTest()
    {
        InitArrays(); // For the first test run per class (xUnit does not wait for the non-static ctor to finish when executing the method in MemberData)
    }

    protected BaseTest()
    {
        InitArrays(); // For reinitialization for each test run
    }

    protected static FragmentedMemory<TElement> CreateFragmentedMemory<TArray, TElement>(TestCaseInput<TArray, TElement> input)
    {
        if (input.Arrays is not null)
        {
            return new FragmentedMemory<TElement>(input.Arrays);
        }

        if (input.Memories is not null)
        {
            return new FragmentedMemory<TElement>(input.Memories);
        }

        throw new InvalidOperationException("Cannot create FragmentedMemory without valid input.");
    }

    protected static void AssertEqualBuffers<T>(T[] actual, int length = 100_000)
    {
        if (actual is byte[] byteBuffer)
        {
            Assert.True(byteBuffer.AsSpan().SequenceEqual(ByteArray100k.AsSpan()[..length]));
        }
        else if (actual is sbyte[] sbyteBuffer)
        {
            Assert.True(sbyteBuffer.AsSpan().SequenceEqual(SByteArray100k.AsSpan()[..length]));
        }
        else if (actual is ushort[] ushortBuffer)
        {
            Assert.True(ushortBuffer.AsSpan().SequenceEqual(UShortArray100k.AsSpan()[..length]));
        }
        else if (actual is short[] shortBuffer)
        {
            Assert.True(shortBuffer.AsSpan().SequenceEqual(ShortArray100k.AsSpan()[..length]));
        }
        else if (actual is uint[] uintBuffer)
        {
            Assert.True(uintBuffer.AsSpan().SequenceEqual(UIntArray100k.AsSpan()[..length]));
        }
        else if (actual is int[] intBuffer)
        {
            Assert.True(intBuffer.AsSpan().SequenceEqual(IntArray100k.AsSpan()[..length]));
        }
        else if (actual is ulong[] ulongBuffer)
        {
            Assert.True(ulongBuffer.AsSpan().SequenceEqual(ULongArray100k.AsSpan()[..length]));
        }
        else if (actual is long[] longBuffer)
        {
            Assert.True(longBuffer.AsSpan().SequenceEqual(LongArray100k.AsSpan()[..length]));
        }
        else
        {
            throw new InvalidOperationException("Invalid array type.");
        }
    }

    public static IEnumerable<object[]> FilterArrayCases(Type? arrayType, Type? elementType, bool includeReadSizes)
    {
        foreach (object[] parameter in GetAllArrayCases())
        {
            if (parameter[0].GetType().GetGenericTypeDefinition() != typeof(TestCaseInput<,>)) continue;

            Type[] genericTypeParams = parameter[0].GetType().GenericTypeArguments;

            if (arrayType != null && genericTypeParams[0] != arrayType) continue;
            if (elementType != null && genericTypeParams[1] != elementType) continue;

            if (includeReadSizes)
            {
                foreach (var readSize in FragmentAndReadSizes)
                {
                    yield return new object[] { parameter[0], readSize };
                }
            }
            else
            {
                yield return parameter;
            }
        }
    }

    private static object[][] GetAllArrayCases()
    {
        var parameters = new object[8 * 2 * FragmentAndReadSizes.Length][];
        int i = 0;

        foreach (var fragmentSize in FragmentAndReadSizes)
        {
            parameters[i++] = new object[] { new TestCaseInput<Memory<Any>, byte>(null, CreateMemoryFragments(ByteArray100k.AsMemory(), fragmentSize)) };
            parameters[i++] = new object[] { new TestCaseInput<Array, byte>(CreateArrayFragments(ByteArray100k.AsMemory(), fragmentSize), null) };

            parameters[i++] = new object[] { new TestCaseInput<Memory<Any>, sbyte>(null, CreateMemoryFragments(SByteArray100k.AsMemory(), fragmentSize)) };
            parameters[i++] = new object[] { new TestCaseInput<Array, sbyte>(CreateArrayFragments(SByteArray100k.AsMemory(), fragmentSize), null) };

            parameters[i++] = new object[] { new TestCaseInput<Memory<Any>, ushort>(null, CreateMemoryFragments(UShortArray100k.AsMemory(), fragmentSize)) };
            parameters[i++] = new object[] { new TestCaseInput<Array, ushort>(CreateArrayFragments(UShortArray100k.AsMemory(), fragmentSize), null) };

            parameters[i++] = new object[] { new TestCaseInput<Memory<Any>, short>(null, CreateMemoryFragments(ShortArray100k.AsMemory(), fragmentSize)) };
            parameters[i++] = new object[] { new TestCaseInput<Array, short>(CreateArrayFragments(ShortArray100k.AsMemory(), fragmentSize), null) };

            parameters[i++] = new object[] { new TestCaseInput<Memory<Any>, uint>(null, CreateMemoryFragments(UIntArray100k.AsMemory(), fragmentSize)) };
            parameters[i++] = new object[] { new TestCaseInput<Array, uint>(CreateArrayFragments(UIntArray100k.AsMemory(), fragmentSize), null) };

            parameters[i++] = new object[] { new TestCaseInput<Memory<Any>, int>(null, CreateMemoryFragments(IntArray100k.AsMemory(), fragmentSize)) };
            parameters[i++] = new object[] { new TestCaseInput<Array, int>(CreateArrayFragments(IntArray100k.AsMemory(), fragmentSize), null) };

            parameters[i++] = new object[] { new TestCaseInput<Memory<Any>, ulong>(null, CreateMemoryFragments(ULongArray100k.AsMemory(), fragmentSize)) };
            parameters[i++] = new object[] { new TestCaseInput<Array, ulong>(CreateArrayFragments(ULongArray100k.AsMemory(), fragmentSize), null) };

            parameters[i++] = new object[] { new TestCaseInput<Memory<Any>, long>(null, CreateMemoryFragments(LongArray100k.AsMemory(), fragmentSize)) };
            parameters[i++] = new object[] { new TestCaseInput<Array, long>(CreateArrayFragments(LongArray100k.AsMemory(), fragmentSize), null) };
        }

        return parameters;
    }

    private static Memory<T>[] CreateMemoryFragments<T>(Memory<T> input, int fragmentSize)
    {
        var result = new Memory<T>[(int)Math.Ceiling(input.Length / (double)fragmentSize)];
        int offset = 0;
        int i = 0;

        for (; i < result.Length - 1; ++i, offset += fragmentSize)
        {
            result[i] = input.Slice(offset, Math.Min(fragmentSize, input.Length));
        }

        if (i < result.Length)
            result[i] = input[offset..];

        return result;
    }

    private static T[][] CreateArrayFragments<T>(Memory<T> input, int fragmentSize)
    {
        var result = new T[(int)Math.Ceiling(input.Length / (double)fragmentSize)][];
        int offset = 0;
        int i = 0;

        for (; i < result.Length - 1; ++i, offset += fragmentSize)
        {
            result[i] = input.Slice(offset, Math.Min(fragmentSize, input.Length)).ToArray();
        }

        if (i < result.Length)
            result[i] = input[offset..].ToArray();

        return result;
    }

    private static TTo[] ReinterpretArray<TTo>(byte[] array)
    {
        ref byte fromArray = ref MemoryMarshal.GetArrayDataReference(array);
        ref TTo toArray = ref Unsafe.As<byte, TTo>(ref fromArray);
        Span<TTo> toSpan = MemoryMarshal.CreateSpan(ref toArray, array.Length / Unsafe.SizeOf<TTo>());

        return toSpan.ToArray();
    }

    private static void InitArrays()
    {
        ByteArray100k = File.ReadAllBytes("Defragmentation\\Inputs\\input-100k.txt");
        SByteArray100k = ReinterpretArray<sbyte>(ByteArray100k);
        UShortArray100k = ReinterpretArray<ushort>(ByteArray100k).Concat(ReinterpretArray<ushort>(ByteArray100k)).ToArray();
        ShortArray100k = ReinterpretArray<short>(ByteArray100k).Concat(ReinterpretArray<short>(ByteArray100k)).ToArray();
        UIntArray100k = ReinterpretArray<uint>(ByteArray100k).Concat(ReinterpretArray<uint>(ByteArray100k)).Concat(ReinterpretArray<uint>(ByteArray100k)).Concat(ReinterpretArray<uint>(ByteArray100k)).ToArray();
        IntArray100k = ReinterpretArray<int>(ByteArray100k).Concat(ReinterpretArray<int>(ByteArray100k)).Concat(ReinterpretArray<int>(ByteArray100k)).Concat(ReinterpretArray<int>(ByteArray100k)).ToArray();
        ULongArray100k = ReinterpretArray<ulong>(ByteArray100k).Concat(ReinterpretArray<ulong>(ByteArray100k)).Concat(ReinterpretArray<ulong>(ByteArray100k)).Concat(ReinterpretArray<ulong>(ByteArray100k)).Concat(ReinterpretArray<ulong>(ByteArray100k)).Concat(ReinterpretArray<ulong>(ByteArray100k)).Concat(ReinterpretArray<ulong>(ByteArray100k)).Concat(ReinterpretArray<ulong>(ByteArray100k)).ToArray();
        LongArray100k = ReinterpretArray<long>(ByteArray100k).Concat(ReinterpretArray<long>(ByteArray100k)).Concat(ReinterpretArray<long>(ByteArray100k)).Concat(ReinterpretArray<long>(ByteArray100k)).Concat(ReinterpretArray<long>(ByteArray100k)).Concat(ReinterpretArray<long>(ByteArray100k)).Concat(ReinterpretArray<long>(ByteArray100k)).Concat(ReinterpretArray<long>(ByteArray100k)).ToArray();
    }

    protected readonly record struct TestCaseInput<TArray, TElement>(TElement[][]? Arrays, Memory<TElement>[]? Memories);

    protected readonly struct Any { }
}
