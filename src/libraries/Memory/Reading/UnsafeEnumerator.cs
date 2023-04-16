using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ofella.Utilities.Memory.Reading;

/// <summary>
/// An enumerator that uses managed pointers to iterate over an array as quickly as possible.
/// </summary>
/// <typeparam name="T">The type of the elements in the underlying array.</typeparam>
public ref struct UnsafeEnumerator<T>
{
    private readonly ref readonly T _boundary; // The element after the last element. Must never be dereferenced. Only used for its address in comparing.
    private ref T _current; // The current element.

    /// <summary>
    /// The current element. Always check the <see cref="IsInBounds"/> property before dereferencing it.
    /// </summary>
    public readonly ref T Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _current;
    }

    /// <summary>
    /// Creates an <see cref="UnsafeEnumerator{T}"/> from the specified array.
    /// </summary>
    /// <param name="array">The array to create the <see cref="UnsafeEnumerator{T}"/> from.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeEnumerator(T[] array)
    {
        _current = ref MemoryMarshal.GetArrayDataReference(array);
        _boundary = ref Unsafe.Add(ref _current, array.Length);
    }

    /// <summary>
    /// Creates an <see cref="UnsafeEnumerator{T}"/> from the specified <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="span">The <see cref="Span{T}"/> to create the <see cref="UnsafeEnumerator{T}"/> from.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeEnumerator(Span<T> span)
    {
        _current = ref MemoryMarshal.GetReference(span);
        _boundary = ref Unsafe.Add(ref _current, span.Length);
    }

    /// <summary>
    /// Creates an <see cref="UnsafeEnumerator{T}"/> from the specified <see cref="Memory{T}"/>.
    /// </summary>
    /// <param name="memory">The <see cref="Memory{T}"/> to create the <see cref="UnsafeEnumerator{T}"/> from.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeEnumerator(Memory<T> memory)
    {
        _current = ref MemoryMarshal.GetReference(memory.Span);
        _boundary = ref Unsafe.Add(ref _current, memory.Length);
    }

    /// <summary>
    /// Indicates whether the current element is in bounds and can be safely dereferenced.
    /// </summary>
    public bool IsInBounds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Unsafe.IsAddressLessThan(ref _current, ref Unsafe.AsRef(in _boundary));
    }

    /// <summary>
    /// Forwards the enumerator by the specified number of elements.
    /// </summary>
    /// <param name="numberOfElements">The number of elements by which the enumerator should be forwarded.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Forward(int numberOfElements)
    {
        _current = ref Unsafe.Add(ref _current, numberOfElements);
    }
}
