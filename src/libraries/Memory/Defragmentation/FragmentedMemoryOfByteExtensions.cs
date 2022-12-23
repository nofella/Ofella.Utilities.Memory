namespace Ofella.Utilities.Memory.Defragmentation;

public static class FragmentedMemoryOfByteExtensions
{
    public static void CopyTo(this FragmentedMemory<byte> fragmentedMemory, Stream stream)
    {
        fragmentedMemory.CopyTo((memory, destinationOffset) => stream.Write(memory.Span));
    }

    public static ValueTask CopyToAsync(this FragmentedMemory<byte> fragmentedMemory, Stream stream, CancellationToken cancellationToken = default)
    {
        return fragmentedMemory.CopyToAsync((memory, destinationOffset, ct) => stream.WriteAsync(memory, ct), cancellationToken);
    }

    public static FragmentedMemoryReaderStream AsStream(this FragmentedMemory<byte> fragmentedMemory) => new(fragmentedMemory);
}
