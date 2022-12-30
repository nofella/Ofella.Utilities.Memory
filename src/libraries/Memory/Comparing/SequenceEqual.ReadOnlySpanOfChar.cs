using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ofella.Utilities.Memory.Comparing;

public static partial class Comparer
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualWithMoreBranches(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        // Pointless to check equality if lengths are not matched
        if (left.Length != right.Length) return false;

        return SequenceEqualWithMoreBranches(
            ref MemoryMarshal.GetReference(left),
            ref MemoryMarshal.GetReference(right),
            (nuint)left.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualWithJumpTable(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        // Pointless to check equality if lengths are not matched
        if (left.Length != right.Length) return false;

        return SequenceEqualWithJumpTable(
            ref MemoryMarshal.GetReference(left),
            ref MemoryMarshal.GetReference(right),
            (nuint)left.Length);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualShort(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        // Pointless to check equality if lengths are not matched
        if (left.Length != right.Length) return false;

        return SequenceEqualShort(
            ref MemoryMarshal.GetReference(left),
            ref MemoryMarshal.GetReference(right),
            (nuint)left.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualShortUnsafe(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        // Pointless to check equality if lengths are not matched
        if (left.Length != right.Length) return false;

        return SequenceEqualShortUnsafe(
            ref MemoryMarshal.GetReference(left),
            ref MemoryMarshal.GetReference(right),
            (nuint)left.Length);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualMedium(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        // Pointless to check equality if lengths are not matched
        if (left.Length != right.Length) return false;

        return SequenceEqualMedium(
            ref MemoryMarshal.GetReference(left),
            ref MemoryMarshal.GetReference(right),
            (nuint)left.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualMediumUnsafe(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        // Pointless to check equality if lengths are not matched
        if (left.Length != right.Length) return false;

        return SequenceEqualMediumUnsafe(
            ref MemoryMarshal.GetReference(left),
            ref MemoryMarshal.GetReference(right),
            (nuint)left.Length);
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualLong(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        // Pointless to check equality if lengths are not matched
        if (left.Length != right.Length) return false;

        return SequenceEqualLong(
            ref MemoryMarshal.GetReference(left),
            ref MemoryMarshal.GetReference(right),
            (nuint)left.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool SequenceEqualLongUnsafe(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        // Pointless to check equality if lengths are not matched
        if (left.Length != right.Length) return false;

        return SequenceEqualLongUnsafe(
            ref MemoryMarshal.GetReference(left),
            ref MemoryMarshal.GetReference(right),
            (nuint)left.Length);
    }
}
