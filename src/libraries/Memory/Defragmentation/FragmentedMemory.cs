using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ofella.Utilities.Memory.Defragmentation;

public static class FragmentedMemory
{
    #region Defragmentation of Memory<T>[]

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy<T>(Memory<T>[] sources, Memory<T> destination) => Copy(sources, destination.Span);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy<T>(Memory<T>[] sources, T[] destination) => Copy(sources, destination.AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy<T>(Memory<T>[] sources, Span<T> destination)
    {
        ref var source = ref MemoryMarshal.GetArrayDataReference(sources);
        ref T currentDestination = ref MemoryMarshal.GetReference(destination);
        ref var endOfSources = ref Unsafe.Add(ref source, sources.Length);

        for (;
            Unsafe.IsAddressLessThan(ref source, ref endOfSources);
            source = ref Unsafe.Add(ref source, 1), currentDestination = ref Unsafe.Add(ref currentDestination, source.Length))
        {
            source.Span.CopyTo(MemoryMarshal.CreateSpan(ref currentDestination, source.Length));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy(Memory<byte>[] sources, Stream destination)
    {
        ref var source = ref MemoryMarshal.GetArrayDataReference(sources);
        ref var endOfSources = ref Unsafe.Add(ref source, sources.Length);

        for (;
            Unsafe.IsAddressLessThan(ref source, ref endOfSources);
            source = ref Unsafe.Add(ref source, 1))
        {
            destination.Write(source.Span);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask CopyAsync(Memory<byte>[] sources, Stream destination, CancellationToken cancellationToken = default)
    {
        foreach (var source in sources)
        {
            await destination.WriteAsync(source, cancellationToken);
        }
    }

    #endregion

    #region Defragmentation of T[][]

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy<T>(T[][] sources, Memory<T> destination) => Copy(sources, destination.Span);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy<T>(T[][] sources, T[] destination) => Copy(sources, destination.AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy<T>(T[][] sources, Span<T> destination)
    {
        ref T[] source = ref MemoryMarshal.GetArrayDataReference(sources);
        ref T currentDestination = ref MemoryMarshal.GetReference(destination);
        ref T[] endOfSources = ref Unsafe.Add(ref source, sources.Length);

        for (;
            Unsafe.IsAddressLessThan(ref source, ref endOfSources);
            source = ref Unsafe.Add(ref source, 1), currentDestination = ref Unsafe.Add(ref currentDestination, source.Length))
        {
            source.CopyTo(MemoryMarshal.CreateSpan(ref currentDestination, source.Length));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy(byte[][] sources, Stream destination)
    {
        ref byte[] source = ref MemoryMarshal.GetArrayDataReference(sources);
        ref byte[] endOfSources = ref Unsafe.Add(ref source, sources.Length);

        for (;
            Unsafe.IsAddressLessThan(ref source, ref endOfSources);
            source = ref Unsafe.Add(ref source, 1))
        {
            destination.Write(source, 0, source.Length);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask CopyAsync(byte[][] sources, Stream destination, CancellationToken cancellationToken = default)
    {
        foreach (var source in sources)
        {
            await destination.WriteAsync(source, cancellationToken);
        }
    }

    #endregion
}
