using BenchmarkDotNet.Attributes;
using Ofella.Utilities.Memory.Defragmentation;

namespace Ofella.Utilities.Memory.Benchmark.Scenarios;

[MemoryDiagnoser]
public class Copying
{
    private const int FragmentCount = 1_000;
    private const int FragmentSize = 1_000_000;
    private readonly Memory<byte>[] _memories;
    private readonly byte[] _buffer;

    public Copying()
    {
        _memories = new Memory<byte>[FragmentCount];
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
}
