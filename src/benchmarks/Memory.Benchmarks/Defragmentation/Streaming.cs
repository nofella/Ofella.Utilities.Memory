using BenchmarkDotNet.Attributes;
using Ofella.Utilities.Memory.Benchmarks.Defragmentation;
using Ofella.Utilities.Memory.Defragmentation;

namespace Ofella.Utilities.Memory.Benchmark.Defragmentation;

[MemoryDiagnoser]
public class Streaming : DefragmentationBase
{
    [Benchmark]
    [Arguments(100)]
    public void Read(int readSize)
    {
        var fragmentedMemory = new FragmentedMemory<byte>(Fragments100k);
        var stream = fragmentedMemory.AsStream();

        int offset = 0;
        int bytesRead;

        while ((bytesRead = stream.Read(Buffer100k, offset, readSize)) > 0) offset += bytesRead;
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
