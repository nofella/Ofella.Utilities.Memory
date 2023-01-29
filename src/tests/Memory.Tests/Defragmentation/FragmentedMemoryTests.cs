using System.IO;
using System;
using Xunit;
using Ofella.Utilities.Memory.Defragmentation;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ofella.Utilities.Memory.Tests.Defragmentation;

public class FragmentedMemoryTests
{
    private readonly Memory<byte> _input100k;

    public FragmentedMemoryTests()
    {
        _input100k = File.ReadAllBytes("Defragmentation\\Inputs\\input-100k.txt");
    }

    public static IEnumerable<object[]> TestData()
    {
        yield return new object[] { 1 };
        yield return new object[] { 10 };
        yield return new object[] { 100 };
        yield return new object[] { 1_000 };
        yield return new object[] { 10_000 };
        yield return new object[] { 100_000 };
        yield return new object[] { 32 };
        yield return new object[] { 64 };
        yield return new object[] { 128 };
        yield return new object[] { 512 };
        yield return new object[] { 950 };
        yield return new object[] { 1024 };
        yield return new object[] { 4096 };
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void CopyMemoryArrayToByteArray(int fragmentSize)
    {
        var fragments = CreateFixLengthFragments(_input100k, fragmentSize);
        var buffer = new byte[100_000];

        FragmentedMemory.Copy(fragments, buffer);

        Assert.True(_input100k.Span.SequenceEqual(buffer));
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public async Task CopyMemoryArrayToByteArrayAsync(int fragmentSize)
    {
        var fragments = CreateFixLengthFragments(_input100k, fragmentSize);
        var buffer = new byte[100_000];

        await FragmentedMemory.CopyAsync(fragments, buffer);

        Assert.True(_input100k.Span.SequenceEqual(buffer));
    }

    private static Memory<byte>[] CreateFixLengthFragments(Memory<byte> input, int fragmentSize)
    {
        var result = new Memory<byte>[(int)Math.Ceiling(input.Length / (double)fragmentSize)];
        int offset = 0;
        int i = 0;

        for (; i < result.Length - 1; ++i, offset += fragmentSize)
        {
            result[i] = input.Slice(offset, fragmentSize);
        }

        result[i] = input[offset..];

        return result;
    }
}
