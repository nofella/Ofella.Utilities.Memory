using Ofella.Utilities.Memory.ManagedPointers;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ofella.Utilities.Memory.Defragmentation;

/// <summary>
/// Abstracts multiple instances of <see cref="Memory{T}"/> or <see cref="T:Byte[]"/> as a single contiguous sequence, without copying them anywhere.
/// </summary>
/// <typeparam name="T">The type of the elements in the <see cref="FragmentedMemory{T}"/>.</typeparam>
public readonly struct FragmentedMemory<T> : IDisposable
{
    #region Fields & Properties

    private readonly MemoryFragment<T>[] _fragments; // Keeps track of the offsets of the fragments, so that they can be binary searched.
    private readonly int _fragmentCount; // Used for boundary checking, since _fragments comes from an ArrayPool which probably causing its size to be greater than requested.
    private readonly int _offset; // The starting offset when an original instance is sliced.
    private readonly DisposeManager _disposeManager;

    /// <summary>
    /// Supports the disposing of a <see cref="FragmentedMemory{T}"/> by tracking its slices that refer to the same memory fragments.
    /// </summary>
    private class DisposeManager
    {
        private int _objectCount = 1;

        /// <summary>
        /// Called when a new instance is formed based on an existing <see cref="FragmentedMemory{T}"/>. Currently, the only case for this is Slicing.
        /// </summary>
        public void Increment()
        {
            Interlocked.Increment(ref _objectCount);
        }

        /// <summary>
        /// Called by a <see cref="FragmentedMemory{T}"/>'s dispose method.
        /// </summary>
        /// <returns>True, when it is called on the last remaining <see cref="FragmentedMemory{T}"/> among instances that refer to the same memory fragments.</returns>
        public bool TryDispose()
        {
            if (Interlocked.Decrement(ref _objectCount) == 0)
            {
                return true;
            }

            return false;
        }
    }

    // This property is not a field to decrease the size of the FragmentedMemory<T> structure.
    // Since the values are actually readonly (both the input and the ones in the struct), it never gets allocated.
    private FragmentedMemoryEnumerator FinishedEnumerator => new(_fragmentCount, 0);

    /// <summary>
    /// The sum of the length of the instance's fragments.
    /// </summary>
    public int Length { get; init; }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates an instance of <see cref="FragmentedMemory{T}"/> by providing its <paramref name="memories"/> as instances of <see cref="Memory{T}"/>.
    /// </summary>
    /// <param name="memories">Fragments of memory as <see cref="Memory{T}"/> to abstract as a single contiguous sequence.</param>
    public FragmentedMemory(Memory<T>[] memories)
    {
        _fragments = ArrayPool<MemoryFragment<T>>.Shared.Rent(memories.Length);
        _fragmentCount = memories.Length;
        _offset = 0;
        _disposeManager = new DisposeManager();

        ref var memory = ref Ptr.Get(memories);
        ref var memoriesBoundary = ref Unsafe.Add(ref memory, memories.Length);
        ref var fragment = ref Ptr.Get(_fragments);
        ref var fragmentsBoundary = ref Unsafe.Add(ref fragment, _fragments.Length);

        for (;
            Unsafe.IsAddressLessThan(ref memory, ref memoriesBoundary);
            memory = ref Unsafe.Add(ref memory, 1), fragment = ref Unsafe.Add(ref fragment, 1))
        {
            fragment = new(memory, Length);
            Length += memory.Length; // Length is calculated by incrementing it by the current memory's length, therefore Length is the current fragment's offset too.

            // Using a simple checked block won't provide any meaningful error message as to where and why the problem happened.
            // Using a checked block with try-catch hurts performance, and is an anti-pattern for flow control.
            // Worst-case scenario is adding int.MaxValue to int.MaxValue, which is still less than 0 (-2), so we cannot miss an overflow using the below condition.
            if (Length < 0)
            {
                throw new ArgumentException($"The combined length of the provided memories exceeds the maximum allowed length '{int.MaxValue}'. Overflow happened at index '{Unsafe.ByteOffset(ref Ptr.Get(memories), ref memory) / Unsafe.SizeOf<Memory<T>>()}'.", nameof(memories));
            }
        }
    }

    /// <summary>
    /// Creates an instance of <see cref="FragmentedMemory{T}"/> by providing its <paramref name="arrays"/> as array of <see cref="T:T[]"/>.
    /// </summary>
    /// <param name="arrays">Fragments of memory as <see cref="T:T[]"/> to abstract as a single contiguous sequence.</param>
    public FragmentedMemory(T[][] arrays)
    {
        _fragments = ArrayPool<MemoryFragment<T>>.Shared.Rent(arrays.Length);
        _fragmentCount = arrays.Length;
        _offset = 0;
        _disposeManager = new DisposeManager();

        ref T[] array = ref Ptr.Get(arrays);
        ref T[] arraysBoundary = ref Unsafe.Add(ref array, arrays.Length);
        ref var fragment = ref Ptr.Get(_fragments);
        ref var fragmentsBoundary = ref Unsafe.Add(ref fragment, _fragments.Length);

        for (;
            Unsafe.IsAddressLessThan(ref array, ref arraysBoundary);
            array = ref Unsafe.Add(ref array, 1), fragment = ref Unsafe.Add(ref fragment, 1))
        {
            fragment = new(array, Length);
            Length += array.Length; // Length is calculated by incrementing it by the current array's length, therefore Length is the current fragment's offset too.

            // Using a simple checked block won't provide any meaningful error message as to where and why the problem happened.
            // Using a checked block with try-catch hurts performance, and is an anti-pattern for flow control.
            // Worst-case scenario is adding int.MaxValue to int.MaxValue, which is still less than 0 (-2), so we cannot miss an overflow using the below condition.
            if (Length < 0)
            {
                throw new ArgumentException($"The combined length of the provided arrays exceeds the maximum allowed length '{int.MaxValue}'. Overflow happened at index '{Unsafe.ByteOffset(ref Ptr.Get(arrays), ref array) / Unsafe.SizeOf<T[]>()}'.", nameof(arrays));
            }
        }
    }

    /// <summary>
    /// Creates an instance of <see cref="FragmentedMemory{T}"/> by slicing another instance using an <paramref name="offset"/> and a <paramref name="length"/>.
    /// </summary>
    /// <param name="fragmentedMemory">The instance to slice.</param>
    /// <param name="offset">The (inclusive) offset to slice from.</param>
    /// <param name="length">The length of the slice.</param>
    private FragmentedMemory(in FragmentedMemory<T> fragmentedMemory, int offset, int length)
    {
        if (offset < 0)
        {
            goto OffsetTooSmall; // Unfavor this branch (forward jmp) when BPU has not enough information.
        }

        if (length <= 0)
        {
            goto LengthTooSmall; // Unfavor this branch (forward jmp) when BPU has not enough information.
        }

        if ((offset + length) > fragmentedMemory.Length)
        {
            goto OutOfBounds; // Unfavor this branch (forward jmp) when BPU has not enough information.
        }

        _fragments = fragmentedMemory._fragments;
        _fragmentCount = fragmentedMemory._fragmentCount;
        _offset = fragmentedMemory._offset + offset;
        _disposeManager = fragmentedMemory._disposeManager;
        Length = length;

        _disposeManager.Increment();

        return;

    OffsetTooSmall:
        throw new ArgumentException($"The value '{offset}' of argument '{nameof(offset)}' must not be less than 0.", nameof(offset));

    LengthTooSmall:
        throw new ArgumentException($"The value '{length}' of argument '{nameof(length)}' must be greater than 0.", nameof(length));

    OutOfBounds:
        throw new ArgumentException($"The boundary '{offset + length}' of the slice '{nameof(offset)} + {nameof(length)}' must not be greater than the current length '{fragmentedMemory.Length}'.");
    }

    /// <summary>
    /// Returns the array to the pool that describes the fragments of this <see cref="FragmentedMemory{T}"/>.
    /// </summary>
    /// <remarks>When slices exist based on a <see cref="FragmentedMemory{T}"/> instance, they are sharing the same memory fragment descriptors, so the actual disposing will only take place when the Dispose method has been called on all instances. This does not apply to the copies of the instances, in which case Disposing a copy would Dispose the other copies as well. (This is the same behavior that one would expect from a class too, but it's a bit strange in case of structs, that are actually being copied.)</remarks>
    public void Dispose()
    {
        if (!_disposeManager.TryDispose())
        {
            return;
        }

        ArrayPool<MemoryFragment<T>>.Shared.Return(_fragments);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Forms a slice of the specified length out of the current <see cref="FragmentedMemory{T}"/> starting at the specified index.
    /// </summary>
    /// <param name="offset">The offset at which to begin the slice.</param>
    /// <param name="length">The desired length of the slice.</param>
    /// <returns>A <see cref="FragmentedMemory{T}"/> of <paramref name="length"/> elements starting at <paramref name="offset"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FragmentedMemory<T> Slice(int offset, int length) => new(in this, _offset + offset, length);

    /// <summary>
    /// Copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by the provided <see cref="T:T[]"/>.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedMemoryEnumerator"/>.</returns>
    public FragmentedMemoryEnumerator CopyTo(T[] destination) => CopyTo(ref Ptr.Get(destination), FragmentedMemoryEnumerator.None);

    /// <summary>
    /// Copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by the provided <see cref="T:T[]"/>.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <param name="fragmentedMemoryEnumerator">An enumerator for controlling the starting offset of the copy operation, returned by the CopyTo or CopyToAsync methods.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedMemoryEnumerator"/>.</returns>
    /// /// <remarks>The <paramref name="fragmentedMemoryEnumerator"/> overrides the starting offset of the copy operation (even if the <see cref="FragmentedMemory{T}"/> is the result of a slice), but does not override the <see cref="Length"/>. It's purpose is to help fragment lookup, when copying slices sequentially. In order to specify a correct length for a copy operation that involves a <see cref="FragmentedMemoryEnumerator"/>, a slice should be formed with the correct length and a dummy offset (it's dummy, because it'll get overwritten by the enumerator).</remarks>
    public FragmentedMemoryEnumerator CopyTo(T[] destination, FragmentedMemoryEnumerator fragmentedMemoryEnumerator) => CopyTo(ref Ptr.Get(destination), fragmentedMemoryEnumerator);

    /// <summary>
    /// Copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by the provided <see cref="Memory{T}"/>.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedMemoryEnumerator"/>.</returns>
    public FragmentedMemoryEnumerator CopyTo(Memory<T> destination) => CopyTo(ref Ptr.Get(destination.Span), FragmentedMemoryEnumerator.None);

    /// <summary>
    /// Copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by the provided <see cref="Memory{T}"/>.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <param name="fragmentedMemoryEnumerator">An enumerator for controlling the starting offset of the copy operation, returned by the CopyTo or CopyToAsync methods.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedMemoryEnumerator"/>.</returns>
    /// /// <remarks>The <paramref name="fragmentedMemoryEnumerator"/> overrides the starting offset of the copy operation (even if the <see cref="FragmentedMemory{T}"/> is the result of a slice), but does not override the <see cref="Length"/>. It's purpose is to help fragment lookup, when copying slices sequentially. In order to specify a correct length for a copy operation that involves a <see cref="FragmentedMemoryEnumerator"/>, a slice should be formed with the correct length and a dummy offset (it's dummy, because it'll get overwritten by the enumerator).</remarks>
    public FragmentedMemoryEnumerator CopyTo(Memory<T> destination, FragmentedMemoryEnumerator fragmentedMemoryEnumerator) => CopyTo(ref Ptr.Get(destination.Span), fragmentedMemoryEnumerator);

    /// <summary>
    /// Copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by the provided <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedMemoryEnumerator"/>.</returns>
    public FragmentedMemoryEnumerator CopyTo(Span<T> destination) => CopyTo(ref Ptr.Get(destination), FragmentedMemoryEnumerator.None);

    /// <summary>
    /// Copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by the provided <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <param name="fragmentedMemoryEnumerator">An enumerator for controlling the starting offset of the copy operation, returned by the CopyTo or CopyToAsync methods.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedMemoryEnumerator"/>.</returns>
    /// <remarks>The <paramref name="fragmentedMemoryEnumerator"/> overrides the starting offset of the copy operation (even if the <see cref="FragmentedMemory{T}"/> is the result of a slice), but does not override the <see cref="Length"/>. It's purpose is to help fragment lookup, when copying slices sequentially. In order to specify a correct length for a copy operation that involves a <see cref="FragmentedMemoryEnumerator"/>, a slice should be formed with the correct length and a dummy offset (it's dummy, because it'll get overwritten by the enumerator).</remarks>
    public FragmentedMemoryEnumerator CopyTo(Span<T> destination, FragmentedMemoryEnumerator fragmentedMemoryEnumerator) => CopyTo(ref Ptr.Get(destination), fragmentedMemoryEnumerator);

    /// <summary>
    /// Asynchronously copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by a managed pointer.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedMemoryEnumerator"/>.</returns>
    public ValueTask<FragmentedMemoryEnumerator> CopyToAsync(Stream destination, CancellationToken cancellationToken = default) => CopyToAsync(destination, FragmentedMemoryEnumerator.None, cancellationToken);

    /// <summary>
    /// Asynchronously copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by a managed pointer.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <param name="fragmentedMemoryEnumerator">An enumerator for controlling the starting offset of the copy operation, returned by the CopyTo or CopyToAsync methods.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedMemoryEnumerator"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown when the generic type parameter of the underlying <see cref="FragmentedMemory{T}"/> is not <see cref="byte"/>.</exception>
    /// <remarks>The <paramref name="fragmentedMemoryEnumerator"/> overrides the starting offset of the copy operation (even if the <see cref="FragmentedMemory{T}"/> is the result of a slice), but does not override the <see cref="Length"/>. It's purpose is to help fragment lookup, when copying slices sequentially. In order to specify a correct length for a copy operation that involves a <see cref="FragmentedMemoryEnumerator"/>, a slice should be formed with the correct length and a dummy offset (it's dummy, because it'll get overwritten by the enumerator).</remarks>
    public async ValueTask<FragmentedMemoryEnumerator> CopyToAsync(Stream destination, FragmentedMemoryEnumerator fragmentedMemoryEnumerator, CancellationToken cancellationToken = default)
    {
        // This is a JIT compile time check, no branching instruction will be emitted.
        if (_fragments is not MemoryFragment<byte>[] fragmentsOfByte)
        {
            throw new NotSupportedException("This method can only be used when the fragments are of type Memory<byte>, since the Stream type only supports byte streams.");
        }

        var startingPosition = fragmentedMemoryEnumerator != FragmentedMemoryEnumerator.None
            ? fragmentedMemoryEnumerator
            : GetFragmentedMemoryEnumerator(_offset);

        if (startingPosition == FinishedEnumerator)
        {
            goto EndOfStream;
        }

        // Copy from first fragment: it needs special handling because it doesn't necessarily start from 0.
        var firstFragmentMemory = fragmentsOfByte[startingPosition.FragmentNo].Memory[startingPosition.OffsetFromFragment..];
        int copyCount = Math.Min(firstFragmentMemory.Length, Length);

        await destination.WriteAsync(firstFragmentMemory[..copyCount], cancellationToken);

        int currentFragmentNo = startingPosition.FragmentNo + 1;
        int destinationOffset = copyCount;

        // Copy from subsequent fragments if needed.
        for (;
            destinationOffset < Length;
            ++currentFragmentNo, destinationOffset += copyCount)
        {
            copyCount = Math.Min(fragmentsOfByte[currentFragmentNo].Memory.Length, Length - destinationOffset);
            await destination.WriteAsync(fragmentsOfByte[currentFragmentNo].Memory[..copyCount], cancellationToken);
        }

        // If destinationOffset > copyCount we copied from more than one fragment, so the currentPosition would simply be the copyCount.
        // Otherwise, we should add startingPosition.Offset to the copyCount, because we copied from the first fragment only,
        // and copying from the 1st one does not necessarily starts from its beginning.
        // Without branching: var currentPosition = (destinationOffset > copyCount ? 0 : startingPosition.Offset) + copyCount
        var currentPosition = (startingPosition.OffsetFromFragment & (((copyCount - destinationOffset) >>> 31) - 1)) + copyCount;

        // If the last copied fragment's length is equal to the currentPosition, then we copied the whole fragment, thus the next position is the 0th offset of the next fragment.
        return _fragments[currentFragmentNo - 1].Memory.Length != currentPosition
            ? new(currentFragmentNo - 1, currentPosition)
            : new(currentFragmentNo, 0);

    EndOfStream:
        return FinishedEnumerator;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Copies the memory fragments represented by this <see cref="FragmentedMemory{T}"/> instance to a contiguous region of memory represented by a managed pointer.
    /// </summary>
    /// <param name="destination">The contiguous region of memory to copy to.</param>
    /// <param name="fragmentedMemoryEnumerator">An enumerator for controlling the starting offset of the copy operation, returned by the CopyTo or CopyToAsync methods.</param>
    /// <returns>The position after the last element of this <see cref="FragmentedMemory{T}"/> as a <see cref="FragmentedMemoryEnumerator"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal FragmentedMemoryEnumerator CopyTo(ref T destination, FragmentedMemoryEnumerator fragmentedMemoryEnumerator)
    {
        var startingPosition = fragmentedMemoryEnumerator != FragmentedMemoryEnumerator.None
            ? fragmentedMemoryEnumerator
            : GetFragmentedMemoryEnumerator(_offset);

        if (startingPosition == FinishedEnumerator)
        {
            goto EndOfStream;
        }

        ref var fragments = ref Ptr.Get(_fragments);
        ref var currentFragment = ref Unsafe.Add(ref fragments, startingPosition.FragmentNo);

        // Copy from first fragment: it needs special handling because it doesn't necessarily start from 0.
        var firstFragmentMemory = currentFragment.Memory[startingPosition.OffsetFromFragment..];
        int copyCount = Math.Min(firstFragmentMemory.Length, Length);

        Ptr.UnalignedCopy(ref destination, ref Ptr.Get(firstFragmentMemory.Span), copyCount);

        currentFragment = ref Unsafe.Add(ref currentFragment, 1);
        ref T pDestination = ref Unsafe.Add(ref destination, copyCount);
        ref T boundary = ref Unsafe.Add(ref destination, Length);

        // Copy from subsequent fragments if needed.
        for (;
            Unsafe.IsAddressLessThan(ref pDestination, ref boundary);
            currentFragment = ref Unsafe.Add(ref currentFragment, 1), pDestination = ref Unsafe.Add(ref pDestination, copyCount))
        {
            copyCount = Math.Min(currentFragment.Memory.Length, (int)Unsafe.ByteOffset(ref pDestination, ref boundary) / Unsafe.SizeOf<T>());
            Ptr.UnalignedCopy(ref pDestination, ref Ptr.Get(currentFragment.Memory.Span[..copyCount]), copyCount);
        }

        // If destinationOffset > copyCount we copied from more than one fragment, so the currentPosition would simply be the copyCount.
        // Otherwise, we should add startingPosition.Offset to the copyCount, because we copied from the first fragment only,
        // and copying from the 1st one does not necessarily starts from its beginning.
        // Without branching: var currentPosition = (destinationOffset > copyCount ? 0 : startingPosition.Offset) + copyCount
        var currentPosition = (startingPosition.OffsetFromFragment & (((copyCount - (int)Unsafe.ByteOffset(ref destination, ref pDestination) / Unsafe.SizeOf<T>()) >>> 31) - 1)) + copyCount;
        var currentFragmentNo = (int)Unsafe.ByteOffset(ref fragments, ref currentFragment) / Unsafe.SizeOf<MemoryFragment<T>>();

        // If the last copied fragment's length is equal to the currentPosition, then we copied the whole fragment, thus the next position is the 0th offset of the next fragment.
        return Unsafe.Subtract(ref currentFragment, 1).Memory.Length != currentPosition
            ? new(currentFragmentNo - 1, currentPosition)
            : new(currentFragmentNo, 0);

    EndOfStream:
        return FinishedEnumerator;
    }

    /// <summary>
    /// Gets the <see cref="FragmentedMemoryEnumerator"/> of a specific offset.
    /// </summary>
    /// <param name="offset">The offset to find the <see cref="FragmentedMemoryEnumerator"/> for.</param>
    /// <returns>A <see cref="FragmentedMemoryEnumerator"/> containing the fragmentNumber and the offset within the fragment.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Only implemented in a separate method for readability. Longer/redundant code is better than emitting a call here.
    private FragmentedMemoryEnumerator GetFragmentedMemoryEnumerator(int offset)
    {
        ref var fragment = ref Ptr.Get(_fragments);

        int lowerBoundary = 0;
        int upperBoundary = _fragmentCount - 1;
        int fragmentNoToCheck;

        // Binary search _fragments for the offset.
        while (lowerBoundary <= upperBoundary)
        {
            fragmentNoToCheck = (lowerBoundary + upperBoundary) >> 1;
            var fragmentToCheck = Unsafe.Add(ref fragment, fragmentNoToCheck);

            if (offset < fragmentToCheck.Offset)
            {
                upperBoundary = fragmentNoToCheck - 1;
            }
            else if (offset > fragmentToCheck.Offset + fragmentToCheck.Memory.Length - 1)
            {
                lowerBoundary = fragmentNoToCheck + 1;
            }
            else
            {
                return new(fragmentNoToCheck, offset - fragmentToCheck.Offset);
            }
        }

        throw new ArgumentException($"The requested offset '{offset}' cannot be found within the MemoryFragment array.", nameof(offset));
    }

    #endregion
}
