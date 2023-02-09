using Ofella.Utilities.Memory.Defragmentation;
using System.Diagnostics;
using Xunit;

namespace Ofella.Utilities.Memory.Tests.Defragmentation;

public class FragmentedMemoryOfTTests : BaseTest
{
    [Theory]
    [MemberData(nameof(FragmentedMemoriesWithReadSizes), false, false, DisableDiscoveryEnumeration = true)]
    protected void CopyToArray<T>(FragmentedMemory<T> fragmentedMemory)
    {
        T[] buffer = new T[100_000];

        fragmentedMemory.CopyTo(buffer);

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FragmentedMemoriesWithReadSizes), false, false, DisableDiscoveryEnumeration = true)]
    protected void CopyToMemory<T>(FragmentedMemory<T> fragmentedMemory)
    {
        T[] buffer = new T[100_000];

        fragmentedMemory.CopyTo(buffer.AsMemory());

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FragmentedMemoriesWithReadSizes), false, false, DisableDiscoveryEnumeration = true)]
    protected void CopyToSpan<T>(FragmentedMemory<T> fragmentedMemory)
    {
        T[] buffer = new T[100_000];

        fragmentedMemory.CopyTo(buffer.AsSpan());

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    protected async Task CopyToStreamAsync(FragmentedMemory<byte> fragmentedMemory)
    {
        byte[] buffer = new byte[100_000];
        using var stream = new MemoryStream(buffer);

        await fragmentedMemory.CopyToAsync(stream);

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FragmentedMemoriesWithReadSizes), true, true, DisableDiscoveryEnumeration = true)]
    protected async Task CopyToStreamAsync_WithReadSizes(FragmentedMemory<byte> fragmentedMemory, int readSize)
    {
        byte[] buffer = new byte[readSize];
        using var stream = new MemoryStream(buffer);

        if (readSize > fragmentedMemory.Length)
        {
            Assert.Throws<InvalidOperationException>(() => fragmentedMemory.Slice(0, readSize));
            return;
        }

        await fragmentedMemory.Slice(0, readSize).CopyToAsync(stream);

        AssertEqualBuffers(buffer, readSize);
    }

    [Theory]
    [MemberData(nameof(FragmentedMemoriesWithReadSizes), false, false, DisableDiscoveryEnumeration = true)]
    protected void CopyToArray_AtEndOfFragmentedMemory<T>(FragmentedMemory<T> fragmentedMemory)
    {
        T[] buffer = new T[100_000];

        var endOfStreamPosition = fragmentedMemory.CopyTo(buffer);

        var fragmentedMemorySliceAtEnd = fragmentedMemory.Slice(endOfStreamPosition, 10);

        var fragmentedPositionAfterCopy = fragmentedMemorySliceAtEnd.CopyTo(buffer);

        Assert.Equal(endOfStreamPosition, fragmentedPositionAfterCopy);
    }

    [Theory]
    [MemberData(nameof(FragmentedMemoriesWithReadSizes), false, false, DisableDiscoveryEnumeration = true)]
    protected void CopyToArray_AfterEndOfFragmentedMemory<T>(FragmentedMemory<T> fragmentedMemory)
    {
        T[] buffer = new T[100_000];

        var endOfStreamPosition = fragmentedMemory.CopyTo(buffer);

        var fragmentedMemorySliceAfterEnd = fragmentedMemory.Slice(fragmentedMemory.Length + 10, 10);

        var fragmentedPositionAfterCopy = fragmentedMemorySliceAfterEnd.CopyTo(buffer);

        Assert.Equal(endOfStreamPosition, fragmentedPositionAfterCopy);
    }

    [Theory]
    [MemberData(nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    protected async Task CopyToStreamAsync_AtEndOfFragmentedMemory(FragmentedMemory<byte> fragmentedMemory)
    {
        byte[] buffer = new byte[100_000];
        using var stream = new MemoryStream(buffer);

        var endOfStreamPosition = await fragmentedMemory.CopyToAsync(stream);

        var fragmentedMemorySliceAtEnd = fragmentedMemory.Slice(endOfStreamPosition, 10);

        var fragmentedPositionAfterCopy = await fragmentedMemorySliceAtEnd.CopyToAsync(stream);

        Assert.Equal(endOfStreamPosition, fragmentedPositionAfterCopy);
    }

    [Theory]
    [MemberData(nameof(FragmentedMemoriesWithReadSizes), true, false, DisableDiscoveryEnumeration = true)]
    protected async Task CopyToStreamAsync_AfterEndOfFragmentedMemory(FragmentedMemory<byte> fragmentedMemory)
    {
        byte[] buffer = new byte[100_000];
        using var stream = new MemoryStream(buffer);

        var endOfStreamPosition = await fragmentedMemory.CopyToAsync(stream);

        var fragmentedMemorySliceAfterEnd = fragmentedMemory.Slice(fragmentedMemory.Length + 10, 10);

        var fragmentedPositionAfterCopy = await fragmentedMemorySliceAfterEnd.CopyToAsync(stream);

        Assert.Equal(endOfStreamPosition, fragmentedPositionAfterCopy);
    }

    [Fact]
    protected async Task DoNotAllow_CopyToStreamAsync_WhenNotByte_Short()
    {
        byte[] buffer = new byte[100];
        using var stream = new MemoryStream(buffer);

        var fragmentedMemory = new FragmentedMemory<short>(new short[][] { new short[] { 1, 2, 3 } });

        await Assert.ThrowsAsync<NotSupportedException>(() => fragmentedMemory.CopyToAsync(stream).AsTask());
    }

    [Fact]
    protected async Task DoNotAllow_CopyToStreamAsync_WhenNotByte_Int()
    {
        byte[] buffer = new byte[100];
        using var stream = new MemoryStream(buffer);

        var fragmentedMemory = new FragmentedMemory<int>(new int[][] { new int[] { 1, 2, 3 } });

        await Assert.ThrowsAsync<NotSupportedException>(() => fragmentedMemory.CopyToAsync(stream).AsTask());
    }

    [Fact]
    protected async Task DoNotAllow_CopyToStreamAsync_WhenNotByte_Long()
    {
        byte[] buffer = new byte[100];
        using var stream = new MemoryStream(buffer);

        var fragmentedMemory = new FragmentedMemory<long>(new long[][] { new long[] { 1, 2, 3 } });

        await Assert.ThrowsAsync<NotSupportedException>(() => fragmentedMemory.CopyToAsync(stream).AsTask());
    }

    [Fact]
    protected void DoNotAllow_SliceByFragmentedPosition_WhenAlreadySliced()
    {
        var fragmentedMemory = new FragmentedMemory<byte>(new byte[][] { new byte[] { 1, 2, 3 } });
        var slicedFragmentedMemory = fragmentedMemory.Slice(new FragmentedPosition(0, 2), 1);

        Assert.Throws<InvalidOperationException>(() => slicedFragmentedMemory.Slice(FragmentedPosition.Beginning, 2));
    }

    //[Fact]
    //protected void DoNotAllow_SliceMoreThanAvailable()
    //{
    //    var fragmentedMemory = new FragmentedMemory<byte>(new byte[][] { new byte[] { 1, 2, 3 } });
    //    var slicedFragmentedMemory = fragmentedMemory.Slice(new FragmentedPosition(0, 2), 1);

    //    Assert.Throws<InvalidOperationException>(() => slicedFragmentedMemory.Slice(FragmentedPosition.Beginning, 2));
    //}
}
