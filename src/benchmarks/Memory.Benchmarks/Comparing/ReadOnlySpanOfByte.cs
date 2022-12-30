using BenchmarkDotNet.Attributes;
using Ofella.Utilities.Memory.Comparing;

namespace Ofella.Utilities.Memory.Benchmarks.Comparing;

[MemoryDiagnoser]
public class ReadOnlySpanOfByte
{
    #region Benchmark Config

    public static readonly int[] Sizes = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 32, 64, 128, 256 };

    public Memory<byte>[] _left;
    public Memory<byte>[] _right;

    public ReadOnlySpanOfByte()
    {
        _left = new Memory<byte>[Sizes.Length];
        _right = new Memory<byte>[Sizes.Length];

        _left[0] = new byte[] { 0 };
        _right[0] = new byte[] { 1 };

        for (var i = 1; i < Sizes.Length; ++i)
        {
            var left = new byte[Sizes[i]];
            left[^1] = 1;
            _left[i] = left;

            var right = new byte[Sizes[i]];
            right[^1] = 1;
            _right[i] = right;
        }
    }
    public IEnumerable<object[]> GetParams()
    {
        yield return new object[] { _left[0], _right[0] };
        yield return new object[] { _left[1], _right[1] };
        yield return new object[] { _left[2], _right[2] };
        yield return new object[] { _left[3], _right[3] };
        yield return new object[] { _left[4], _right[4] };
        yield return new object[] { _left[5], _right[5] };
        yield return new object[] { _left[6], _right[6] };
        yield return new object[] { _left[7], _right[7] };
        yield return new object[] { _left[8], _right[8] };
        yield return new object[] { _left[9], _right[9] };
        yield return new object[] { _left[10], _right[10] };
        yield return new object[] { _left[11], _right[11] };
        //yield return new object[] { _strLeft[12], _strRight[12] };
        //yield return new object[] { _strLeft[13], _strRight[13] };
        //yield return new object[] { _strLeft[14], _strRight[14] };
        //yield return new object[] { _strLeft[15], _strRight[15] };
        //yield return new object[] { _strLeft[16], _strRight[16] };
    }

    #endregion

    [Benchmark]
    [ArgumentsSource(nameof(GetParams))]
    public bool OptimizedSequenceEqual(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        return Comparer.SequenceEqual(left, right);
    }

    [Benchmark]
    [ArgumentsSource(nameof(GetParams))]
    public bool EqualityOperator_NonConstant(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        return left.SequenceEqual(right);
    }
}
