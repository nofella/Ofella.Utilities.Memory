using BenchmarkDotNet.Attributes;
using Ofella.Utilities.Memory.Defragmentation;
using System.Runtime.CompilerServices;

namespace Ofella.Utilities.Memory.Benchmark.Scenarios;

[MemoryDiagnoser]
public class Copying
{
    private const int FragmentCount = 1_000;
    private const int FragmentSize = 64_000;
    private readonly byte[][] _memories;
    private readonly byte[] _buffer;

    public Copying()
    {
        _memories = new byte[FragmentCount][];
        _buffer = new byte[FragmentCount * FragmentSize];

        for (var i = 0; i < FragmentCount; ++i)
        {
            _memories[i] = new byte[FragmentSize];
        }
    }

    [Benchmark]
    public void UsingCopyFromFragmentedMemory()
    {
        FragmentedMemory.Copy(_memories, _buffer);
    }

    [Benchmark]
    public async Task UsingMemoryStream()
    {
        using var memoryStream = new MemoryStream(_buffer);

        foreach(var memory in _memories)
        {
            await memoryStream.WriteAsync(memory);
        }
    }

    [Benchmark]
    public void UsingMemoryStreamWithSpans()
    {
        using var memoryStream = new MemoryStream(_buffer);

        foreach (var memory in _memories)
        {
            memoryStream.Write(memory);
        }
    }
}
