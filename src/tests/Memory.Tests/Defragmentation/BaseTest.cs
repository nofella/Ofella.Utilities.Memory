using Ofella.Utilities.Memory.Defragmentation;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace Ofella.Utilities.Memory.Tests.Defragmentation;

public abstract class BaseTest
{
    private static readonly int[] FragmentAndReadSizes = new int[] { 1, 10, 100, 1_000, 10_000, 100_000, 32, 64, 128, 512, 950, 1024, 4096, 33_333, 64_112, 150_000 };

    protected static byte[] ByteArray100k;
    protected static sbyte[] SByteArray100k;
    protected static ushort[] UShortArray100k;
    protected static short[] ShortArray100k;
    protected static uint[] UIntArray100k;
    protected static int[] IntArray100k;
    protected static ulong[] ULongArray100k;
    protected static long[] LongArray100k;

    static BaseTest()
    {
        InitArrays(); // For the first test run per class (xUnit does not wait for the non-static ctor to finish when executing the method in MemberData)
    }

    protected BaseTest()
    {
        InitArrays(); // For reinitialization for each test run
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

    public static void AssertEqualBuffers<T>(T[] actual, int length = 100_000)
    {
        if (actual is byte[] byteBuffer)
        {
            Assert.True(byteBuffer.AsSpan().SequenceEqual(ByteArray100k.AsSpan()[..length]));
        }
        else if (actual is short[] shortBuffer)
        {
            Assert.True(shortBuffer.AsSpan().SequenceEqual(ShortArray100k.AsSpan()[..length]));
        }
        else
        {
            throw new InvalidOperationException("Invalid array type.");
        }
    }

    public static IEnumerable<object[]> SeekTestData()
    {
        yield return new object[] { 0 };

        foreach (var readSize in FragmentAndReadSizes)
        {
            yield return new object[] { readSize };
        }
    }

    public static IEnumerable<object[]> FragmentedMemoriesWithReadSizes(bool onlyByte, bool includeReadSizes)
    {
        foreach (object[] fragmentedMemoryWithReadSize in FragmentedMemoriesWithReadSizesFromArray(ByteArray100k, includeReadSizes))
        {
            yield return fragmentedMemoryWithReadSize;
        }

        if (!onlyByte)
        {
            foreach (object[] fragmentedMemoryWithReadSize in FragmentedMemoriesWithReadSizesFromArray(ShortArray100k, includeReadSizes))
            {
                yield return fragmentedMemoryWithReadSize;
            }
        }
    }

    public static IEnumerable<object[]> FilterArrayCases(Type? arrayType, Type? elementType)
    {
        foreach (object[] parameter in GetAllArrayCases())
        {
            if (parameter[0].GetType().GetGenericTypeDefinition() != typeof(TestCaseInput<,>)) continue;

            Type[] genericTypeParams = parameter[0].GetType().GenericTypeArguments;

            if (arrayType != null && (genericTypeParams[0] != arrayType)) continue;
            if (elementType != null && genericTypeParams[1] != elementType) continue;

            yield return parameter;
        }
    }

    public static object[][] GetAllArrayCases()
    {
        var parameters = new object[2 * 2 * FragmentAndReadSizes.Length][];
        int i = 0;

        foreach (var fragmentSize in FragmentAndReadSizes)
        {
            parameters[i++] = new object[] { new TestCaseInput<Memory<Any>, byte>(null, CreateMemoryFragments(ByteArray100k.AsMemory(), fragmentSize)) };
            parameters[i++] = new object[] { new TestCaseInput<Array, byte>(CreateArrayFragments(ByteArray100k.AsMemory(), fragmentSize), null) };
            parameters[i++] = new object[] { new TestCaseInput<Memory<Any>, short>(null, CreateMemoryFragments(ShortArray100k.AsMemory(), fragmentSize)) };
            parameters[i++] = new object[] { new TestCaseInput<Array, short>(CreateArrayFragments(ShortArray100k.AsMemory(), fragmentSize), null) };
        }

        return parameters;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerable<FragmentedMemory<T>> GetFragmentedMemories<T>(T[] array, int length, int fragmentSize)
    {
        T[] slicedArray = length > 0 ? array.Take(length).ToArray() : array.Take(array.Length).ToArray();

        yield return new FragmentedMemory<T>(CreateArrayFragments(slicedArray.AsMemory(), fragmentSize));
        yield return new FragmentedMemory<T>(CreateMemoryFragments(slicedArray.AsMemory(), fragmentSize));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerable<object[]> FragmentedMemoriesWithReadSizesFromArray<T>(T[] array, bool includeReadSizes)
    {
        foreach (var fragmentSize in FragmentAndReadSizes)
        {
            foreach (var fragmentedMemory in GetFragmentedMemories<T>(array, -1, fragmentSize))
            {
                if (includeReadSizes)
                {
                    foreach (var readSize in FragmentAndReadSizes)
                    {
                        yield return new object[] { fragmentedMemory, readSize };
                    }
                }
                else
                {
                    yield return new object[] { fragmentedMemory };
                }
            }
        }
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

    protected readonly record struct TestCaseInput<TArray, TElement>(TElement[][]? Arrays, Memory<TElement>[]? Memories);

    protected readonly struct Any { }
}
