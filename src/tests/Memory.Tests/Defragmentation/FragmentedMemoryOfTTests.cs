using Ofella.Utilities.Memory.Defragmentation;
using System.Diagnostics;
using Xunit;

namespace Ofella.Utilities.Memory.Tests.Defragmentation;

public class FragmentedMemoryOfTTests : BaseTest
{
    [Theory]
    [MemberData(nameof(FilterArrayCases), null, null, false, DisableDiscoveryEnumeration = true)]
    protected void CopyToArray<TArray, TElement>(TestCaseInput<TArray, TElement> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        TElement[] buffer = new TElement[100_000];

        fragmentedMemory.CopyTo(buffer);

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, null, false, DisableDiscoveryEnumeration = true)]
    protected void CopyToMemory<TArray, TElement>(TestCaseInput<TArray, TElement> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        TElement[] buffer = new TElement[100_000];

        fragmentedMemory.CopyTo(buffer.AsMemory());

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, null, false, DisableDiscoveryEnumeration = true)]
    protected void CopyToSpan<TArray, TElement>(TestCaseInput<TArray, TElement> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        TElement[] buffer = new TElement[100_000];

        fragmentedMemory.CopyTo(buffer.AsSpan());

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected async Task CopyToStreamAsync<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        byte[] buffer = new byte[100_000];
        using var stream = new MemoryStream(buffer);

        await fragmentedMemory.CopyToAsync(stream);

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), true, DisableDiscoveryEnumeration = true)]
    protected async Task CopyToStreamAsync_WithReadSizes<TArray>(TestCaseInput<TArray, byte> input, int readSize)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
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
    [MemberData(nameof(FilterArrayCases), null, null, false, DisableDiscoveryEnumeration = true)]
    protected void CopyToArray_AtEndOfFragmentedMemory<TArray, TElement>(TestCaseInput<TArray, TElement> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        TElement[] buffer = new TElement[100_000];

        var endOfStreamPosition = fragmentedMemory.CopyTo(buffer);

        using var fragmentedMemorySliceAtEnd = fragmentedMemory.Slice(endOfStreamPosition, 10);

        var fragmentedPositionAfterCopy = fragmentedMemorySliceAtEnd.CopyTo(buffer);

        Assert.Equal(endOfStreamPosition, fragmentedPositionAfterCopy);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, null, false, DisableDiscoveryEnumeration = true)]
    protected void CopyToArray_AfterEndOfFragmentedMemory<TArray, TElement>(TestCaseInput<TArray, TElement> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        TElement[] buffer = new TElement[100_000];

        var endOfStreamPosition = fragmentedMemory.CopyTo(buffer);

        using var fragmentedMemorySliceAfterEnd = fragmentedMemory.Slice(fragmentedMemory.Length + 10, 10);

        var fragmentedPositionAfterCopy = fragmentedMemorySliceAfterEnd.CopyTo(buffer);

        Assert.Equal(endOfStreamPosition, fragmentedPositionAfterCopy);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected async Task CopyToStreamAsync_AtEndOfFragmentedMemory<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        byte[] buffer = new byte[100_000];
        using var stream = new MemoryStream(buffer);

        var endOfStreamPosition = await fragmentedMemory.CopyToAsync(stream);

        using var fragmentedMemorySliceAtEnd = fragmentedMemory.Slice(endOfStreamPosition, 10);

        var fragmentedPositionAfterCopy = await fragmentedMemorySliceAtEnd.CopyToAsync(stream);

        Assert.Equal(endOfStreamPosition, fragmentedPositionAfterCopy);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected async Task CopyToStreamAsync_AfterEndOfFragmentedMemory<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        byte[] buffer = new byte[100_000];
        using var stream = new MemoryStream(buffer);

        var endOfStreamPosition = await fragmentedMemory.CopyToAsync(stream);

        using var fragmentedMemorySliceAfterEnd = fragmentedMemory.Slice(fragmentedMemory.Length + 10, 10);

        var fragmentedPositionAfterCopy = await fragmentedMemorySliceAfterEnd.CopyToAsync(stream);

        Assert.Equal(endOfStreamPosition, fragmentedPositionAfterCopy);
    }

    [Fact]
    protected async Task DoNotAllow_CopyToStreamAsync_WhenNotByte_Short()
    {
        byte[] buffer = new byte[100];
        using var stream = new MemoryStream(buffer);

        using var fragmentedMemory = new FragmentedMemory<short>(new short[][] { new short[] { 1, 2, 3 } });

        await Assert.ThrowsAsync<NotSupportedException>(() => fragmentedMemory.CopyToAsync(stream).AsTask());
    }

    [Fact]
    protected async Task DoNotAllow_CopyToStreamAsync_WhenNotByte_Int()
    {
        byte[] buffer = new byte[100];
        using var stream = new MemoryStream(buffer);

        using var fragmentedMemory = new FragmentedMemory<int>(new int[][] { new int[] { 1, 2, 3 } });

        await Assert.ThrowsAsync<NotSupportedException>(() => fragmentedMemory.CopyToAsync(stream).AsTask());
    }

    [Fact]
    protected async Task DoNotAllow_CopyToStreamAsync_WhenNotByte_Long()
    {
        byte[] buffer = new byte[100];
        using var stream = new MemoryStream(buffer);

        using var fragmentedMemory = new FragmentedMemory<long>(new long[][] { new long[] { 1, 2, 3 } });

        await Assert.ThrowsAsync<NotSupportedException>(() => fragmentedMemory.CopyToAsync(stream).AsTask());
    }

    [Fact]
    protected void DoNotAllow_SliceByFragmentedPosition_WhenAlreadySliced()
    {
        using var fragmentedMemory = new FragmentedMemory<byte>(new byte[][] { new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 } });
        var slicedFragmentedMemory = fragmentedMemory.Slice(new FragmentedPosition(0, 2), 5);

        Assert.Throws<InvalidOperationException>(() => slicedFragmentedMemory.Slice(FragmentedPosition.Beginning, 2));
    }

    [Fact]
    protected void DoNotAllow_SliceMoreThanAvailable()
    {
        using var fragmentedMemory = new FragmentedMemory<byte>(new byte[][] { new byte[] { 1, 2, 3 } });

        Assert.Throws<InvalidOperationException>(() => fragmentedMemory.Slice(new FragmentedPosition(0, 2), 30));
    }
}
