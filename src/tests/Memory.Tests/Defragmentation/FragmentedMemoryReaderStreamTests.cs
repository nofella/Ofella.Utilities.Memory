using Xunit;
using Ofella.Utilities.Memory.Defragmentation;

namespace Ofella.Utilities.Memory.Tests.Defragmentation;

public class FragmentedMemoryReaderStreamTests : BaseTest
{
    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, true, DisableDiscoveryEnumeration = true)]
    public void Read(FragmentedMemory<byte> fragmentedMemory, int readSize)
    {
        var stream = fragmentedMemory.AsStream();

        var buffer = new byte[100_000];
        int offset = 0;
        int bytesRead;

        while ((bytesRead = stream.Read(buffer, offset, readSize)) > 0) offset += bytesRead;

        Assert.True(ByteArray100k.AsSpan().SequenceEqual(buffer));
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    public void ReadByte(FragmentedMemory<byte> fragmentedMemory)
    {
        var stream = fragmentedMemory.AsStream();

        var buffer = new byte[100_000];
        int offset = 0;
        int byteValue;

        while ((byteValue = stream.ReadByte()) != -1)
        {
            buffer[offset++] = (byte)byteValue;
        }

        Assert.True(ByteArray100k.AsSpan().SequenceEqual(buffer));
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    public void CopyTo(FragmentedMemory<byte> fragmentedMemory)
    {
        var stream = fragmentedMemory.AsStream();

        var buffer = new byte[100_000];
        using var memoryStream = new MemoryStream(buffer);

        stream.CopyTo(memoryStream);

        Assert.True(ByteArray100k.AsSpan().SequenceEqual(buffer));
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    public async Task CopyToAsync(FragmentedMemory<byte> fragmentedMemory)
    {
        var stream = fragmentedMemory.AsStream();

        var buffer = new byte[100_000];
        using var memoryStream = new MemoryStream(buffer);

        await stream.CopyToAsync(memoryStream);

        Assert.True(ByteArray100k.AsSpan().SequenceEqual(buffer));
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, true, DisableDiscoveryEnumeration = true)] // Using ReadSizes as input for offset
    public void SeekFromBeginning(FragmentedMemory<byte> fragmentedMemory, int offset)
    {
        var stream = fragmentedMemory.AsStream();

        stream.Seek(offset, SeekOrigin.Begin);

        Assert.Equal(offset, stream.Position);
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, true, DisableDiscoveryEnumeration = true)] // Using ReadSizes as input for offset
    public void SeekFromCurrent(FragmentedMemory<byte> fragmentedMemory, int offset)
    {
        var stream = fragmentedMemory.AsStream();

        var previousPosition = stream.Position;
        stream.Seek(offset, SeekOrigin.Current);

        Assert.Equal(previousPosition + offset, stream.Position);
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, true, DisableDiscoveryEnumeration = true)] // Using ReadSizes as input for offset
    public void SeekFromEnd(FragmentedMemory<byte> fragmentedMemory, int offset)
    {
        var stream = fragmentedMemory.AsStream();

        stream.Seek(offset, SeekOrigin.End);

        Assert.Equal(stream.Length - offset, stream.Position);
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, true, DisableDiscoveryEnumeration = true)] // Using ReadSizes as input for offset
    public void SeekFromInvalid(FragmentedMemory<byte> fragmentedMemory, int offset)
    {
        var stream = fragmentedMemory.AsStream();

        var previousPosition = stream.Position;
        stream.Seek(offset, (SeekOrigin)12);

        Assert.Equal(previousPosition, stream.Position);
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    public void CanReadIsTrue(FragmentedMemory<byte> fragmentedMemory)
    {
        var stream = fragmentedMemory.AsStream();

        Assert.True(stream.CanRead);
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    public void CanSeekIsTrue(FragmentedMemory<byte> fragmentedMemory)
    {
        var stream = fragmentedMemory.AsStream();

        Assert.True(stream.CanSeek);
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    public void CanWriteIsFalse(FragmentedMemory<byte> fragmentedMemory)
    {
        var stream = fragmentedMemory.AsStream();

        Assert.False(stream.CanWrite);
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    public void FlushIsNotSupported(FragmentedMemory<byte> fragmentedMemory)
    {
        var stream = fragmentedMemory.AsStream();

        Assert.Throws<NotSupportedException>(() => stream.Flush());
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    public async Task FlushAsyncIsNotSupported(FragmentedMemory<byte> fragmentedMemory)
    {
        var stream = fragmentedMemory.AsStream();

        await Assert.ThrowsAsync<NotSupportedException>(() => stream.FlushAsync());
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    public void SetLengthIsNotSupported(FragmentedMemory<byte> fragmentedMemory)
    {
        var stream = fragmentedMemory.AsStream();

        Assert.Throws<NotSupportedException>(() => stream.SetLength(100));
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    public async Task WriteAsyncIsNotSupported(FragmentedMemory<byte> fragmentedMemory)
    {
        var stream = fragmentedMemory.AsStream();

        await Assert.ThrowsAsync<NotSupportedException>(() => stream.WriteAsync(new byte[10]).AsTask());
    }

    [Theory]
    [MemberData(memberName: nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    public void WriteByteIsNotSupported(FragmentedMemory<byte> fragmentedMemory)
    {
        var stream = fragmentedMemory.AsStream();

        Assert.Throws<NotSupportedException>(() => stream.WriteByte(1));
    }

}
