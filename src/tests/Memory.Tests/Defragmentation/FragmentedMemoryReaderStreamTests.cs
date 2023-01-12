using System.IO;
using System;
using Xunit;
using Ofella.Utilities.Memory.Defragmentation;

namespace Ofella.Utilities.Memory.Tests.Defragmentation;

public class FragmentedMemoryReaderStreamTests
{
    private readonly Memory<byte> _input100k;

    public FragmentedMemoryReaderStreamTests()
    {
        _input100k = File.ReadAllBytes("Defragmentation\\Inputs\\input-100k.txt");
    }

    [Theory]
    // fragmentSize = readSize
    [InlineData(10, 10)]
    [InlineData(100, 100)]
    [InlineData(1_000, 1_000)]
    [InlineData(10_000, 10_000)]

    // fragmentSize > readSize
    [InlineData(1_000, 512)]
    [InlineData(10_000, 1_000)]
    [InlineData(10_000, 100)]
    public void ReadByFragmentSize(int fragmentSize, int readSize)
    {
        var fragments = CreateFixLengthFragments(_input100k, fragmentSize);
        var fragmentedMemory = new FragmentedMemory<byte>(fragments);
        var stream = new FragmentedMemoryReaderStream(fragmentedMemory);

        var buffer = new byte[100_000];
        int offset = 0;
        int bytesRead;

        while ((bytesRead = stream.Read(buffer, offset, readSize)) > 0) offset += bytesRead;

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
