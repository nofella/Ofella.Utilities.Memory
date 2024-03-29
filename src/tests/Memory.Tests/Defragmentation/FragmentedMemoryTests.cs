﻿using Ofella.Utilities.Memory.Defragmentation;
using Xunit;

namespace Ofella.Utilities.Memory.Tests.Defragmentation;

public class FragmentedMemoryTests : BaseTest
{
    [Theory]
    [MemberData(nameof(FilterArrayCases), typeof(Memory<Any>), null, false, DisableDiscoveryEnumeration = true)]
    protected void CopyMemoryArrayToSpan<T>(TestCaseInput<Memory<Any>, T> input)
    {
        T[] buffer = new T[100_000];

        FragmentedMemory.Copy(input.Memories!, buffer.AsSpan());

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), typeof(Memory<Any>), typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected void CopyMemoryArrayToStream(TestCaseInput<Memory<Any>, byte> input)
    {
        using var stream = new MemoryStream(new byte[100_000]);

        FragmentedMemory.Copy(input.Memories!, stream);

        AssertEqualBuffers(stream.ToArray());
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), typeof(Memory<Any>), typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected async Task CopyMemoryArrayToStreamAsync(TestCaseInput<Memory<Any>, byte> input)
    {
        using var stream = new MemoryStream(new byte[100_000]);

        await FragmentedMemory.CopyAsync(input.Memories!, stream);

        AssertEqualBuffers(stream.ToArray());
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), typeof(Array), null, false, DisableDiscoveryEnumeration = true)]
    protected void CopyJaggedArrayToSpan<T>(TestCaseInput<Array, T> input)
    {
        T[] buffer = new T[100_000];

        FragmentedMemory.Copy(input.Arrays!, buffer.AsSpan());

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), typeof(Array), typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected void CopyJaggedArrayToStream(TestCaseInput<Array, byte> input)
    {
        using var stream = new MemoryStream(new byte[100_000]);

        FragmentedMemory.Copy(input.Arrays!, stream);

        AssertEqualBuffers(stream.ToArray());
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), typeof(Array), typeof(byte), false, DisableDiscoveryEnumeration = true)]
    protected async Task CopyJaggedArrayToStreamAsync(TestCaseInput<Array, byte> input)
    {
        using var stream = new MemoryStream(new byte[100_000]);

        await FragmentedMemory.CopyAsync(input.Arrays!, stream);

        AssertEqualBuffers(stream.ToArray());
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), typeof(Memory<Any>), null, false, DisableDiscoveryEnumeration = true)]
    protected async Task CopyMemoryArrayToMemoryParallelAsync<T>(TestCaseInput<Memory<Any>, T> input)
    {
        T[] buffer = new T[100_000];

        await FragmentedMemory.CopyParallelAsync(input.Memories!, buffer.AsMemory());

        AssertEqualBuffers(buffer);
    }

    [Theory]
    [MemberData(nameof(FilterArrayCases), typeof(Array), null, false, DisableDiscoveryEnumeration = true)]
    protected async Task CopyJaggedArrayToMemoryParallelAsync<T>(TestCaseInput<Array, T> input)
    {
        T[] buffer = new T[100_000];

        await FragmentedMemory.CopyParallelAsync(input.Arrays!, buffer.AsMemory());

        AssertEqualBuffers(buffer);
    }
}
