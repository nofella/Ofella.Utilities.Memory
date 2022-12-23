namespace Ofella.Utilities.Memory.Defragmentation;

internal readonly record struct MemoryFragment<T>(Memory<T> Memory, int Offset);
