using BenchmarkDotNet.Attributes;
using Ofella.Utilities.Memory.Benchmarks.Defragmentation;
using Ofella.Utilities.Memory.Defragmentation;

namespace Ofella.Utilities.Memory.Benchmark.Defragmentation;

[MemoryDiagnoser]
public class Copying : DefragmentationBase
{
    public IEnumerable<object[]> Arguments()
    {
        //yield return new object[] { Fragments100k, Buffer100k };
        //yield return new object[] { Fragments100M, Buffer100M };
        yield return new object[] { Fragments1G, Buffer1G };
    }

    [Benchmark]
    [ArgumentsSource(nameof(Arguments))]
    public void UsingCopyFromFragmentedMemory(Memory<byte>[] fragments, byte[] buffer)
    {
        FragmentedMemory.Copy(fragments, buffer);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Arguments))]
    public Task UsingAsyncCopy(Memory<byte>[] fragments, byte[] buffer)
    {
        return FragmentedMemory.CopyAsync(fragments, buffer);
    }

    //[Benchmark]
    public async Task UsingMemoryStream(Memory<byte>[] fragments, byte[] buffer)
    {
        using var memoryStream = new MemoryStream(buffer);

        foreach (var fragment in fragments)
        {
            await memoryStream.WriteAsync(fragment);
        }
    }

    //[Benchmark]
    public void UsingMemoryStreamWithSpans(Memory<byte>[] fragments, byte[] buffer)
    {
        using var memoryStream = new MemoryStream(buffer);

        foreach (var fragment in fragments)
        {
            memoryStream.Write(fragment.Span);
        }
    }
}
