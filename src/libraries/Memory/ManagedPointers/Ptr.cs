using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ofella.Utilities.Memory.ManagedPointers;

public static class Ptr
{
    /// <summary>
    /// Helper method for getting a managed pointer to the 0th element of an array without checking boundaries.
    /// </summary>
    /// <typeparam name="TElement">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to return a managed pointer to.</param>
    /// <returns>A managed pointer to the 0th element of the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Get<T>(T[] array) => ref MemoryMarshal.GetArrayDataReference(array);

    /// <summary>
    /// Helper method for getting a managed pointer to the 0th element of a span without checking boundaries.
    /// </summary>
    /// <typeparam name="TElement">The type of the elements in the span.</typeparam>
    /// <param name="array">The span to return a managed pointer to.</param>
    /// <returns>A managed pointer to the 0th element of the span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Get<T>(Span<T> span) => ref MemoryMarshal.GetReference(span);

    /// <summary>
    /// Generalized Unsafe.CopyBlock. This method does not (actually cannot) check boundaries.
    /// </summary>
    /// <typeparam name="T">The type of the items that the destination and source managed pointers point to.</typeparam>
    /// <param name="destination">The managed pointer corresponding to the destination address to copy to.</param>
    /// <param name="source">The managed pointer corresponding to the source address to copy from.</param>
    /// <param name="count">The number of items to copy.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnalignedCopy<T>(ref T destination, ref T source, int count)
    {
        Unsafe.CopyBlockUnaligned(
            ref Unsafe.As<T, byte>(ref destination),
            ref Unsafe.As<T, byte>(ref source),
            (uint)(count * Unsafe.SizeOf<T>()));
    }
}
