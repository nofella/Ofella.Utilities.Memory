using Ofella.Utilities.Memory.ManagedPointers;
using System.Runtime.CompilerServices;

namespace Ofella.Utilities.Memory.Defragmentation;

/// <summary>
/// Provides methods for simple defragmentation of memory without having to create a <see cref="FragmentedMemory{T}"/>.
/// </summary>
public static class FragmentedMemory
{
    #region Defragmentation of Memory<T>[]

    /// <summary>
    /// Defragments a region of memory represented by multiple <see cref="Memory{T}"/> instances by copying them into a contiguous region of memory represented by a <see cref="Span{T}"/> instance in the provided order.
    /// </summary>
    /// <typeparam name="T">The type of the elements in <paramref name="sources"/> and <paramref name="destination"/>.</typeparam>
    /// <param name="sources">The fragmented memory region.</param>
    /// <param name="destination">A contiguous region of memory to defragment into.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Prefer redundant code in memory rather than emitting a call.
    public static void Copy<T>(Memory<T>[] sources, Span<T> destination)
    {
        ref var source = ref Ptr.Get(sources);
        ref T currentDestination = ref Ptr.Get(destination);
        ref var boundary = ref Unsafe.Add(ref source, sources.Length); // For perf: the boundary is the address AFTER the last element, therefore it MUST NEVER be dereferenced.

        for (;
            Unsafe.IsAddressLessThan(ref source, ref boundary);
            currentDestination = ref Unsafe.Add(ref currentDestination, source.Length), source = ref Unsafe.Add(ref source, 1))
        {
            Ptr.UnalignedCopy(ref currentDestination, ref Ptr.Get(source.Span), source.Length);
        }
    }

    /// <summary>
    /// Defragments a region of memory represented by multiple <see cref="Memory{T}"/> instances by writing them into a <see cref="Stream"/> instance in the provided order.
    /// </summary>
    /// <param name="sources">The fragmented memory region.</param>
    /// <param name="destination">The stream to defragment into.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Prefer redundant code in memory rather than emitting a call.
    public static void Copy(Memory<byte>[] sources, Stream destination)
    {
        ref var source = ref Ptr.Get(sources);
        ref var boundary = ref Unsafe.Add(ref source, sources.Length); // For perf: the boundary is the address AFTER the last element, therefore it MUST NEVER be dereferenced.

        // Fastest way to loop in NET7
        for (;
            Unsafe.IsAddressLessThan(ref source, ref boundary);
            source = ref Unsafe.Add(ref source, 1))
        {
            destination.Write(source.Span);
        }
    }

    /// <summary>
    /// Defragments a region of memory represented by multiple <see cref="Memory{T}"/> instances by asynchronously writing them into a <see cref="Stream"/> instance in the provided order.
    /// </summary>
    /// <param name="sources">The fragmented memory region.</param>
    /// <param name="destination">The stream to defragment into.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous copy operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Prefer redundant code in memory rather than emitting a call.
    public static async ValueTask CopyAsync(Memory<byte>[] sources, Stream destination, CancellationToken cancellationToken = default)
    {
        foreach (var source in sources)
        {
            await destination.WriteAsync(source, cancellationToken);
        }
    }

    #endregion

    #region Defragmentation of T[][]

    /// <summary>
    /// Defragments a region of memory represented by <see cref="T:byte[]"/> instances by copying them into a contiguous region of memory represented by a <see cref="Span{T}"/> instance in the provided order.
    /// </summary>
    /// <typeparam name="T">The type of the elements in <paramref name="sources"/> and <paramref name="destination"/>.</typeparam>
    /// <param name="sources">The fragmented memory region.</param>
    /// <param name="destination">A contiguous region of memory to defragment into.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Prefer redundant code in memory rather than emitting a call.
    public static void Copy<T>(T[][] sources, Span<T> destination)
    {
        // Types are specified explicitly, because the implicitly determined ones are wrong (T[]? and T?)
        ref T[] source = ref Ptr.Get(sources);
        ref T currentDestination = ref Ptr.Get(destination);
        ref T[] boundary = ref Unsafe.Add(ref source, sources.Length); // For perf: the boundary is the address AFTER the last element, therefore it MUST NEVER be dereferenced.

        // Fastest way to loop in NET7
        for (;
            Unsafe.IsAddressLessThan(ref source, ref boundary);
            currentDestination = ref Unsafe.Add(ref currentDestination, source.Length), source = ref Unsafe.Add(ref source, 1))
        {
            Ptr.UnalignedCopy(ref currentDestination, ref Ptr.Get(source), source.Length);
        }
    }

    /// <summary>
    /// Defragments a region of memory represented by <see cref="T:byte[]"/> instances by writing them into a <see cref="Stream"/> instance in the provided order.
    /// </summary>
    /// <param name="sources">The fragmented memory region.</param>
    /// <param name="destination">The stream to defragment into.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Prefer redundant code in memory rather than emitting a call.
    public static void Copy(byte[][] sources, Stream destination)
    {
        // Types are specified explicitly, because the implicitly determined ones are wrong (byte[])
        ref byte[] source = ref Ptr.Get(sources);
        ref byte[] boundary = ref Unsafe.Add(ref source, sources.Length); // For perf: the boundary is the address AFTER the last element, therefore it MUST NEVER be dereferenced.

        // Fastest way to loop in NET7
        for (;
            Unsafe.IsAddressLessThan(ref source, ref boundary);
            source = ref Unsafe.Add(ref source, 1))
        {
            destination.Write(source, 0, source.Length);
        }
    }

    /// <summary>
    /// Defragments a region of memory represented by <see cref="T:byte[]"/> instances by asynchronously writing them into a <see cref="Stream"/> instance in the provided order.
    /// </summary>
    /// <param name="sources">The fragmented memory region.</param>
    /// <param name="destination">The stream to defragment into.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous copy operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Prefer redundant code in memory rather than emitting a call.
    public static async ValueTask CopyAsync(byte[][] sources, Stream destination, CancellationToken cancellationToken = default)
    {
        foreach (var source in sources)
        {
            await destination.WriteAsync(source, cancellationToken);
        }
    }

    #endregion

    #region Parallel Defragmentation

    /// <summary>
    /// Asynchronously defragments a region of memory represented by <see cref="Memory{T}"/> instances by copying them in the provided order to a contiguous destination represented by a single <see cref="Memory{T}"/> instance.
    /// This method operates in a parallel way using 2 concurrent tasks of which the first one copies to the first half of the destination, and the second one copies to the second half of the destination.
    /// </summary>
    /// <typeparam name="T">The type of the elements in <paramref name="sources"/> and <paramref name="destination"/>.</typeparam>
    /// <param name="sources">The fragmented memory region.</param>
    /// <param name="destination">A contiguous region of memory to defragment into.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous copy operation.</returns>
    public static Task CopyParallelAsync<T>(Memory<T>[] sources, Memory<T> destination)
    {
        var fragmentedMemory = new FragmentedMemory<T>(sources);
        var halfSize = fragmentedMemory.Length >>> 1; // Forcing an unsigned division by 2, because we know that it can't be negative.
        var copyJob = CopyAsyncTask<T>; // Needed for correct branch coverage calculation.

        // Unroll 2 copy operations instead of trying to find the max. degree of parallelism based on the number of CPU cores available.
        // It seems that on most systems not the CPU, but the available bandwidth of memory is the real bottleneck,
        // thus it's quite improbable that more than 2 copy operations running in parallel can help us.
        return Task.WhenAll(
            Task.Factory.StartNew(copyJob, new TaskState<T>(fragmentedMemory.Slice(0, halfSize), destination)), // Slice is faster when length is known due to the way FragmentedMemory<T> is implemented.
            Task.Factory.StartNew(copyJob, new TaskState<T>(fragmentedMemory[halfSize..], destination[halfSize..])) // When length is not available there's no perf. difference between Slice and this[], but latter is easier to write.
            );
    }

    /// <summary>
    /// Asynchronously defragments a region of memory represented by instances of <see cref="T:T[]"/> by copying them in the provided order to a contiguous destination represented by a single array of <see cref="T"/>.
    /// This method operates in a parallel way using 2 concurrent tasks of which the first one copies to the first half of the destination, and the second one copies to the second half of the destination.
    /// </summary>
    /// <typeparam name="T">The type of the elements in <paramref name="sources"/> and <paramref name="destination"/>.</typeparam>
    /// <param name="sources">The fragmented memory region.</param>
    /// <param name="destination">A contiguous region of memory to defragment into.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous copy operation.</returns>
    public static Task CopyParallelAsync<T>(T[][] sources, Memory<T> destination)
    {
        var fragmentedMemory = new FragmentedMemory<T>(sources);
        var halfSize = fragmentedMemory.Length >>> 1; // Forcing an unsigned division by 2, because we know that it can't be negative.
        var copyJob = CopyAsyncTask<T>; // Needed for correct branch coverage calculation.

        // Unroll 2 copy operations instead of trying to find the max. degree of parallelism based on the number of CPU cores available.
        // It seems that on most systems not the CPU, but the available bandwidth of memory is the real bottleneck,
        // thus it's quite improbable that more than 2 copy operations running in parallel can help us.
        return Task.WhenAll(
            Task.Factory.StartNew(copyJob, new TaskState<T>(fragmentedMemory.Slice(0, halfSize), destination)), // Slice is faster when length is available due to the way FragmentedMemory<T> is implemented.
            Task.Factory.StartNew(copyJob, new TaskState<T>(fragmentedMemory[halfSize..], destination[halfSize..])) // When length is not available there's no perf. difference between Slice and this[], but latter is easier to write.
            );
    }

    private static void CopyAsyncTask<T>(object? taskState)
    {
        var state = (TaskState<T>)taskState!; // We knew that it can't be null based on the way we use it.
        state.FragmentedMemory.CopyTo(state.Destination);
    }

    private readonly record struct TaskState<T>(FragmentedMemory<T> FragmentedMemory, Memory<T> Destination);

    #endregion
}
