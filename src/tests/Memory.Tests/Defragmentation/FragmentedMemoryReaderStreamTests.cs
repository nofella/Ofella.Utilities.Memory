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
    // edge cases
    [InlineData(1, 1)]
    [InlineData(1, 100_000)]
    [InlineData(100_000, 1)]
    [InlineData(100_000, 100_000)]

    // fragmentSize = readSize
    [InlineData(10, 10)]
    [InlineData(100, 100)]
    [InlineData(1_000, 1_000)]
    [InlineData(10_000, 10_000)]

    // fragmentSize > readSize && fragmentSize % readSize == 0
    [InlineData(10, 1)]
    [InlineData(100, 10)]
    [InlineData(1_000, 10)]
    [InlineData(1_000, 100)]
    [InlineData(10_000, 10)]
    [InlineData(10_000, 100)]
    [InlineData(10_000, 1_000)]

    // fragmentSize > readSize && fragmentSize % readSize > 0
    [InlineData(100, 32)]
    [InlineData(100, 64)]
    [InlineData(1_000, 128)]
    [InlineData(1_000, 512)]
    [InlineData(1_000, 950)]
    [InlineData(10_000, 1024)]
    [InlineData(10_000, 4096)]

    // fragmentSize < readSize && readSize % fragmentSize == 0
    [InlineData(1, 10)]
    [InlineData(10, 100)]
    [InlineData(10, 1_000)]
    [InlineData(100, 1_000)]
    [InlineData(10, 10_000)]
    [InlineData(100, 10_000)]
    [InlineData(1_000, 10_000)]

    // fragmentSize < readSize && readSize % fragmentSize > 0
    [InlineData(32, 100)]
    [InlineData(64, 100)]
    [InlineData(128, 1_000)]
    [InlineData(512, 1_000)]
    [InlineData(950, 1_000)]
    [InlineData(1024, 10_000)]
    [InlineData(4096, 10_000)]
    public void Read(int fragmentSize, int readSize)
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
