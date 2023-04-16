using BenchmarkDotNet.Attributes;
using Ofella.Utilities.Memory.Reading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ofella.Utilities.Memory.Benchmarks.Reading;

[MemoryDiagnoser]
public class Enumeration
{
    private const int ArraySize = 1_000_000;

    private readonly int[] _array32;
    private volatile int _dummy32;

    public Enumeration()
    {
        _array32 = new int[ArraySize];
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void For32()
    {
        for (var i = 0; i < _array32.Length; ++i)
        {
            _dummy32 = _array32[i];
        }
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void ManagedPointers32()
    {
        ref var current = ref MemoryMarshal.GetArrayDataReference(_array32);
        ref var boundary = ref Unsafe.Add(ref current, _array32.Length);

        for (; Unsafe.IsAddressLessThan(ref current, ref boundary); current = ref Unsafe.Add(ref current, 1))
        {
            _dummy32 = current;
        }
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void UnsafeEnumerator32()
    {
        for (var enumerator = new UnsafeEnumerator<int>(_array32);
            enumerator.IsInBounds;
            enumerator.Forward(1))
        {
            _dummy32 = enumerator.Current;
        }
    }
}
