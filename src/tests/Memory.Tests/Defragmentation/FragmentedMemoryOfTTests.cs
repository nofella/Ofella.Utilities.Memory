using Ofella.Utilities.Memory.Defragmentation;
using System.Diagnostics;
using Xunit;

namespace Ofella.Utilities.Memory.Tests.Defragmentation;

public class FragmentedMemoryOfTTests : BaseTest
{
    [Fact]
    protected void DoNotAllow_CreateFragmentedMemory_TooLongArrays()
    {
        var array = new byte[100_000_000];
        var fragments = new byte[30][];

        for (var i = 0; i < fragments.Length; ++i)
        {
            fragments[i] = array;
        }

        var ex = Assert.Throws<ArgumentException>(() => new FragmentedMemory<byte>(fragments));
        Assert.Contains("The combined length of the provided arrays exceeds the maximum allowed length", ex.Message);
    }

    [Fact]
    protected void DoNotAllow_CreateFragmentedMemory_TooLongMemories()
    {
        var array = new byte[100_000_000];
        var fragments = new Memory<byte>[30];

        for (var i = 0; i < fragments.Length; ++i)
        {
            fragments[i] = array;
        }

        var ex = Assert.Throws<ArgumentException>(() => new FragmentedMemory<byte>(fragments));
        Assert.Contains("The combined length of the provided memories exceeds the maximum allowed length", ex.Message);
    }

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
            var ex = Assert.Throws<ArgumentException>(() => fragmentedMemory.Slice(0, readSize));
            Assert.Contains("The boundary", ex.Message);
            Assert.Contains("must not be greater than the current length", ex.Message);
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

        var fragmentedPositionAfterCopy = fragmentedMemory.CopyTo(buffer, endOfStreamPosition);

        Assert.Equal(endOfStreamPosition, fragmentedPositionAfterCopy);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), null, typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected async Task CopyToStreamAsync_AtEndOfFragmentedMemory<TArray>(TestCaseInput<TArray, byte> input)
    {
        using var fragmentedMemory = CreateFragmentedMemory(input);
        byte[] buffer = new byte[100_000];
        using var stream = new MemoryStream(buffer);

        var finishedEnumerator = await fragmentedMemory.CopyToAsync(stream);

        var fragmentedPositionAfterCopy = await fragmentedMemory.CopyToAsync(stream, finishedEnumerator);

        Assert.Equal(finishedEnumerator, fragmentedPositionAfterCopy);
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
    protected void DoNotAllow_Slice_MoreThanAvailable()
    {
        using var fragmentedMemory = new FragmentedMemory<byte>(new byte[][] { new byte[] { 1, 2, 3 } });

        var ex = Assert.Throws<ArgumentException>(() => fragmentedMemory[..100]);
        Assert.Contains("The boundary", ex.Message);
        Assert.Contains("must not be greater than the current length", ex.Message);
    }

    [Fact]
    protected void DoNotAllow_Slice_NegativeOffset()
    {
        using var fragmentedMemory = new FragmentedMemory<byte>(new byte[][] { new byte[] { 1, 2, 3 } });

        var ex = Assert.Throws<ArgumentException>(() => fragmentedMemory.Slice(-100, 1));
        Assert.Contains("The value", ex.Message);
        Assert.Contains("must not be less than 0", ex.Message);
    }

    [Fact]
    protected void DoNotAllow_Slice_NegativeLength()
    {
        using var fragmentedMemory = new FragmentedMemory<byte>(new byte[][] { new byte[] { 1, 2, 3 } });

        var ex = Assert.Throws<ArgumentException>(() => fragmentedMemory.Slice(1, -100));
        Assert.Contains("The value", ex.Message);
        Assert.Contains("must be greater than 0", ex.Message);
    }
}
