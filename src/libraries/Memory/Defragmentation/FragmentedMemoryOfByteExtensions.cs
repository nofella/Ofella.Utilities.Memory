using System.Runtime.CompilerServices;

namespace Ofella.Utilities.Memory.Defragmentation;

public static class FragmentedMemoryOfByteExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FragmentedMemoryReaderStream AsStream(this FragmentedMemory<byte> fragmentedMemory) => new(fragmentedMemory);
}
