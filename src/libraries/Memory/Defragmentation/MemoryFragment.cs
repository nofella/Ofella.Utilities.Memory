namespace Ofella.Utilities.Memory.Defragmentation;

/// <summary>
/// Internal struct for pairing the given <paramref name="Memory"/> to a given <paramref name="Offset"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Memory">The <see cref="Memory{T}"/> instance to pair an offset to.</param>
/// <param name="Offset">The starting offset of <paramref name="Memory"/>.</param>
internal readonly record struct MemoryFragment<T>(Memory<T> Memory, int Offset);
