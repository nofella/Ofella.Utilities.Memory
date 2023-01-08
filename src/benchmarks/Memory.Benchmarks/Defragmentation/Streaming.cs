using BenchmarkDotNet.Attributes;
using Ofella.Utilities.Memory.Defragmentation;
using System.Text;

namespace Ofella.Utilities.Memory.Benchmark.Defragmentation;

[MemoryDiagnoser]
public class Streaming
{
    private static readonly Memory<byte>[] memories = new[]
    {
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(new string('X', 1000)).AsMemory(),
    };

    private static byte[] buffer = new byte[21000];

    //[Benchmark]
    public void UsingMemoryStream()
    {
        using var stream = new MemoryStream(21000);

        for (var i = 0; i < memories.Length; ++i)
        {
            stream.Write(memories[i].Span);
        }

        stream.Read(buffer, 0, 3000);
        stream.Read(buffer, 3000, 3000);
        stream.Read(buffer, 6000, 3000);
        stream.Read(buffer, 9000, 3000);
        stream.Read(buffer, 12000, 3000);
        stream.Read(buffer, 15000, 3000);
        stream.Read(buffer, 18000, 3000);
    }

    //[Benchmark]
    public async ValueTask UsingAsyncMemoryStream()
    {
        using var stream = new MemoryStream(21000);

        for (var i = 0; i < memories.Length; ++i)
        {
            await stream.WriteAsync(memories[i]);
        }

        await stream.ReadAsync(buffer, 0, 3000);
        await stream.ReadAsync(buffer, 3000, 3000);
        await stream.ReadAsync(buffer, 6000, 3000);
        await stream.ReadAsync(buffer, 9000, 3000);
        await stream.ReadAsync(buffer, 12000, 3000);
        await stream.ReadAsync(buffer, 15000, 3000);
        await stream.ReadAsync(buffer, 18000, 3000);
    }

    [Benchmark]
    public void UsingFragmentedMemoryReaderStream()
    {
        var fragmentedMemory = new FragmentedMemory<byte>(memories);
        using var stream = fragmentedMemory.AsStream();

        stream.Read(buffer, 0, 3000);
        stream.Read(buffer, 3000, 3000);
        stream.Read(buffer, 6000, 3000);
        stream.Read(buffer, 9000, 3000);
        stream.Read(buffer, 12000, 3000);
        stream.Read(buffer, 15000, 3000);
        stream.Read(buffer, 18000, 3000);
    }
}
