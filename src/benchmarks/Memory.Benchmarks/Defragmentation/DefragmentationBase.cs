namespace Ofella.Utilities.Memory.Benchmarks.Defragmentation;

public abstract class DefragmentationBase
{
    protected Memory<byte> Input100k { get; }

    protected Memory<byte> Input100M { get; }

    protected Memory<byte> Input1G { get; }

    protected Memory<byte>[] Fragments100k { get; }

    protected Memory<byte>[] Fragments100M { get; }

    protected Memory<byte>[] Fragments1G { get; }

    protected byte[] Buffer100k { get; }

    protected byte[] Buffer100M { get; }

    protected byte[] Buffer1G { get; }

    public DefragmentationBase()
    {
        Input100k = File.ReadAllBytes("Defragmentation\\Inputs\\input-100k.txt");
        Input100M = new byte[100_000_000].AsMemory();
        Input1G = new byte[1_000_000_000].AsMemory();

        Buffer100k = new byte[100_000];
        Buffer100M = new byte[100_000_000];
        Buffer1G = new byte[1_000_000_000];

        for (var i = 0;i < 1_000; ++i)
        {
            Input100k.CopyTo(Input100M[(i * 100_000)..]);
        }

        for (var i = 0; i < 10; ++i)
        {
            Input100M.CopyTo(Input1G[(i * 100_000_000)..]);
        }

        Fragments100k = CreateFixLengthFragments(Input100k, 1_000);
        Fragments100M = CreateFixLengthFragments(Input100M, 1_000_000);
        Fragments1G = CreateFixLengthFragments(Input1G, 10_000_000);
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
}
