using BenchmarkDotNet.Attributes;
using Ofella.Utilities.Memory.Defragmentation;

namespace Ofella.Utilities.Memory.Benchmark.Defragmentation;

[MemoryDiagnoser]
public class Streaming
{
    private readonly Memory<byte> _input100k;
    private readonly Memory<byte>[] _fragments;
    private readonly byte[] _buffer;

    public Streaming()
    {
        _input100k = File.ReadAllBytes("Defragmentation\\Inputs\\input-100k.txt");
        _fragments = CreateFixLengthFragments(_input100k, 10);
        _buffer= new byte[100_000];
    }

    [Benchmark]
    [Arguments(100)]
    public void Read(int readSize)
    {
        var fragmentedMemory = new FragmentedMemory<byte>(_fragments);
        var stream = fragmentedMemory.AsStream();

        int offset = 0;
        int bytesRead;

        while ((bytesRead = stream.Read(_buffer, offset, readSize)) > 0) offset += bytesRead;
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

    //[Benchmark]
    //public void UsingMemoryStream()
    //{
    //    using var stream = new MemoryStream(21000);

    //    for (var i = 0; i < memories.Length; ++i)
    //    {
    //        stream.Write(memories[i].Span);
    //    }

    //    stream.Read(buffer, 0, 3000);
    //    stream.Read(buffer, 3000, 3000);
    //    stream.Read(buffer, 6000, 3000);
    //    stream.Read(buffer, 9000, 3000);
    //    stream.Read(buffer, 12000, 3000);
    //    stream.Read(buffer, 15000, 3000);
    //    stream.Read(buffer, 18000, 3000);
    //}

    //[Benchmark]
    //public async ValueTask UsingAsyncMemoryStream()
    //{
    //    using var stream = new MemoryStream(21000);

    //    for (var i = 0; i < memories.Length; ++i)
    //    {
    //        await stream.WriteAsync(memories[i]);
    //    }

    //    await stream.ReadAsync(buffer, 0, 3000);
    //    await stream.ReadAsync(buffer, 3000, 3000);
    //    await stream.ReadAsync(buffer, 6000, 3000);
    //    await stream.ReadAsync(buffer, 9000, 3000);
    //    await stream.ReadAsync(buffer, 12000, 3000);
    //    await stream.ReadAsync(buffer, 15000, 3000);
    //    await stream.ReadAsync(buffer, 18000, 3000);
    //}
}
