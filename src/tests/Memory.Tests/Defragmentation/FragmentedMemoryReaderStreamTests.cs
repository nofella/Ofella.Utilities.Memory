using System.Text;
using System;
using Xunit;
using Ofella.Utilities.Memory.Defragmentation;

namespace Ofella.Utilities.Memory.Tests.Defragmentation;

public class FragmentedMemoryReaderStreamTests
{
    private const string Source = "";

    private readonly Memory<byte>[] _memories1000 = new[]
    {
        Encoding.UTF8.GetBytes(Source.Substring(0, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(1000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(2000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(3000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(4000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(5000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(6000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(7000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(8000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(9000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(10_000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(11_000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(12_000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(13_000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(14_000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(15_000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(16_000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(17_000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(18_000, 1000)).AsMemory(),
        Encoding.UTF8.GetBytes(Source.Substring(19_000, 1000)).AsMemory()
    };

    [Fact]
    public void ReadByFragmentSize()
    {
        var buffer = new byte[20_000];
        var fragmentedMemory = new FragmentedMemory<byte>(_memories1000);
        var stream = new FragmentedMemoryReaderStream(fragmentedMemory);

        for(int i = 0; i < buffer.Length / 1_000; ++i)
        {
            stream.Read(buffer, i * 1_000, 1000);
        }

        var resultStr = Encoding.UTF8.GetString(buffer);

        Assert.Equal(Source, resultStr);
    }
}
