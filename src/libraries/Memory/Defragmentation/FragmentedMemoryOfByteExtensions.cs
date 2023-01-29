using System.Runtime.CompilerServices;

namespace Ofella.Utilities.Memory.Defragmentation;

public static class FragmentedMemoryOfByteExtensions
{
    /// <summary>
    /// Creates a <see cref="Stream"/> from the underlying <see cref="FragmentedMemory{byte}"/>.
    /// </summary>
    /// <param name="fragmentedMemory">The <see cref="FragmentedMemory{byte}"/> to create a <see cref="Stream"/> from.</param>
    /// <returns>The underlying <see cref="FragmentedMemory{byte}"/> as a <see cref="Stream"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Prefer redundant code in memory rather than emitting a call.
    public static Stream AsStream(this FragmentedMemory<byte> fragmentedMemory) => new FragmentedMemoryReaderStream(fragmentedMemory);
}
