using Xunit;
using Ofella.Utilities.Memory.Defragmentation;

namespace Ofella.Utilities.Memory.Tests.Defragmentation;

public class FragmentedMemoryReaderStreamTests : BaseTest
{
    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), true, DisableDiscoveryEnumeration = true)]
    protected void Read<TArray>(TestCaseInput<TArray, byte> input, int readSize)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        var buffer = new byte[100_000];
        int offset = 0;
        int bytesRead;

        while ((bytesRead = stream.Read(buffer, offset, readSize)) > 0) offset += bytesRead;

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected void ReadByte<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        var buffer = new byte[100_000];
        int offset = 0;
        int byteValue;

        while ((byteValue = stream.ReadByte()) != -1)
        {
            buffer[offset++] = (byte)byteValue;
        }

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected void CopyTo<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        var buffer = new byte[100_000];
        using var memoryStream = new MemoryStream(buffer);

        stream.CopyTo(memoryStream);

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected async Task CopyToAsync<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        var buffer = new byte[100_000];
        using var memoryStream = new MemoryStream(buffer);

        await stream.CopyToAsync(memoryStream);

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), true, DisableDiscoveryEnumeration = true)] // Using ReadSizes as input for offset
    protected void SeekFromBeginning<TArray>(TestCaseInput<TArray, byte> input, int offset)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        stream.Seek(offset, SeekOrigin.Begin);

        Assert.Equal(offset, stream.Position);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), true, DisableDiscoveryEnumeration = true)] // Using ReadSizes as input for offset
    protected void SeekFromCurrent<TArray>(TestCaseInput<TArray, byte> input, int offset)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        var previousPosition = stream.Position;
        stream.Seek(offset, SeekOrigin.Current);

        Assert.Equal(previousPosition + offset, stream.Position);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), true, DisableDiscoveryEnumeration = true)] // Using ReadSizes as input for offset
    protected void SeekFromEnd<TArray>(TestCaseInput<TArray, byte> input, int offset)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        stream.Seek(offset, SeekOrigin.End);

        Assert.Equal(stream.Length - offset, stream.Position);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), true, DisableDiscoveryEnumeration = true)] // Using ReadSizes as input for offset
    protected void SeekFromInvalid<TArray>(TestCaseInput<TArray, byte> input, int offset)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        var previousPosition = stream.Position;
        stream.Seek(offset, (SeekOrigin)12);

        Assert.Equal(previousPosition, stream.Position);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected void CanReadIsTrue<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        Assert.True(stream.CanRead);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected void CanSeekIsTrue<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        Assert.True(stream.CanSeek);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected void CanWriteIsFalse<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        Assert.False(stream.CanWrite);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected void FlushIsNotSupported<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        Assert.Throws<NotSupportedException>(() => stream.Flush());
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected async Task FlushAsyncIsNotSupported<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        await Assert.ThrowsAsync<NotSupportedException>(() => stream.FlushAsync());
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected void SetLengthIsNotSupported<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        Assert.Throws<NotSupportedException>(() => stream.SetLength(100));
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected async Task WriteAsyncIsNotSupported<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        await Assert.ThrowsAsync<NotSupportedException>(() => stream.WriteAsync(new byte[10]).AsTask());
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected void WriteByteIsNotSupported<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        var stream = fragmentedMemory.AsStream();

        Assert.Throws<NotSupportedException>(() => stream.WriteByte(1));
    }

}
