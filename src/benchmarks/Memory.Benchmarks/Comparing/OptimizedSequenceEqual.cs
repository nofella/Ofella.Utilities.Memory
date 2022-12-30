using BenchmarkDotNet.Attributes;
using Ofella.Utilities.Memory.Comparing;
using Ofella.Utilities.Memory.ManagedPointers;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Ofella.Utilities.Memory.Benchmarks.Comparing;

[MemoryDiagnoser]
public class OptimizedSequenceEqual
{
    #region Benchmark Config

    public static readonly int[] Sizes = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 32, 64, 128, 256 };

    public string[] _strLeft1;
    public string[] _strLeft2;
    public string[] _strLeft3;

    public string[] _strRight1;
    public string[] _strRight2;
    public string[] _strRight3;

    private volatile bool _dummy1;
    private volatile bool _dummy2;

    public OptimizedSequenceEqual()
    {
        _strLeft1 = new string[Sizes.Length];
        _strLeft2 = new string[Sizes.Length];
        _strLeft3 = new string[Sizes.Length];

        _strRight1 = new string[Sizes.Length];
        _strRight2 = new string[Sizes.Length];
        _strRight3 = new string[Sizes.Length];

        _strLeft1[0] = "1";
        _strLeft2[0] = "2";
        _strLeft3[0] = "3";

        _strRight1[0] = "4";
        _strRight2[0] = "5";
        _strRight3[0] = "6";

        for (var i = 1; i < Sizes.Length; ++i)
        {
            _strLeft1[i] = new string('x', Sizes[i] - 1) + '0';
            _strLeft2[i] = new string('y', Sizes[i] - 1) + '0';
            _strLeft3[i] = new string('z', Sizes[i] - 1) + '0';
            
            _strRight1[i] = new string('x', Sizes[i] - 1) + '1';
            _strRight2[i] = new string('y', Sizes[i] - 1) + '1';
            _strRight3[i] = new string('z', Sizes[i] - 1) + '1';
        }
    }

    public IEnumerable<object[]> Params()
    {
        yield return new object[] { _strLeft1[0], _strRight1[0], _strLeft2[0], _strRight2[0], _strLeft3[0], _strRight3[0] };
        yield return new object[] { _strLeft1[1], _strRight1[1], _strLeft2[1], _strRight2[1], _strLeft3[1], _strRight3[1] };
        yield return new object[] { _strLeft1[2], _strRight1[2], _strLeft2[2], _strRight2[2], _strLeft3[2], _strRight3[2] };
        yield return new object[] { _strLeft1[3], _strRight1[3], _strLeft2[3], _strRight2[3], _strLeft3[3], _strRight3[3] };
        yield return new object[] { _strLeft1[4], _strRight1[4], _strLeft2[4], _strRight2[4], _strLeft3[4], _strRight3[4] };
        yield return new object[] { _strLeft1[5], _strRight1[5], _strLeft2[5], _strRight2[5], _strLeft3[5], _strRight3[5] };
        yield return new object[] { _strLeft1[6], _strRight1[6], _strLeft2[6], _strRight2[6], _strLeft3[6], _strRight3[6] };
        yield return new object[] { _strLeft1[7], _strRight1[7], _strLeft2[7], _strRight2[7], _strLeft3[7], _strRight3[7] };
        yield return new object[] { _strLeft1[8], _strRight1[8], _strLeft2[8], _strRight2[8], _strLeft3[8], _strRight3[8] };
        yield return new object[] { _strLeft1[9], _strRight1[9], _strLeft2[9], _strRight2[9], _strLeft3[9], _strRight3[9] };
        yield return new object[] { _strLeft1[10], _strRight1[10], _strLeft2[10], _strRight2[10], _strLeft3[10], _strRight3[10] };
        yield return new object[] { _strLeft1[11], _strRight1[11], _strLeft2[11], _strRight2[11], _strLeft3[11], _strRight3[11] };
        yield return new object[] { _strLeft1[12], _strRight1[12], _strLeft2[12], _strRight2[12], _strLeft3[12], _strRight3[12] };
        yield return new object[] { _strLeft1[13], _strRight1[13], _strLeft2[13], _strRight2[13], _strLeft3[13], _strRight3[13] };
        yield return new object[] { _strLeft1[14], _strRight1[14], _strLeft2[14], _strRight2[14], _strLeft3[14], _strRight3[14] };
        yield return new object[] { _strLeft1[15], _strRight1[15], _strLeft2[15], _strRight2[15], _strLeft3[15], _strRight3[15] };
        yield return new object[] { _strLeft1[16], _strRight1[16], _strLeft2[16], _strRight2[16], _strLeft3[16], _strRight3[16] };
        yield return new object[] { _strLeft1[17], _strRight1[17], _strLeft2[17], _strRight2[17], _strLeft3[17], _strRight3[17] };
        yield return new object[] { _strLeft1[18], _strRight1[18], _strLeft2[18], _strRight2[18], _strLeft3[18], _strRight3[18] };
        yield return new object[] { _strLeft1[19], _strRight1[19], _strLeft2[19], _strRight2[19], _strLeft3[19], _strRight3[19] };
    }

    #endregion

    [Benchmark]
    [ArgumentsSource(nameof(Params))]
    public bool OptimizedWithMoreBranches(ReadOnlySpan<char> left1, ReadOnlySpan<char> right1, ReadOnlySpan<char> left2, ReadOnlySpan<char> right2, ReadOnlySpan<char> left3, ReadOnlySpan<char> right3)
    {
        _dummy1 = Comparer.SequenceEqualWithMoreBranches(left1, right1);
        _dummy2 = Comparer.SequenceEqualWithMoreBranches(left2, right2);

        return Comparer.SequenceEqualWithMoreBranches(left3, right3);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Params))]
    public bool OptimizedWithJumpTable(ReadOnlySpan<char> left1, ReadOnlySpan<char> right1, ReadOnlySpan<char> left2, ReadOnlySpan<char> right2, ReadOnlySpan<char> left3, ReadOnlySpan<char> right3)
    {
        _dummy1 = Comparer.SequenceEqualWithJumpTable(left1, right1);
        _dummy2 = Comparer.SequenceEqualWithJumpTable(left2, right2);

        return Comparer.SequenceEqualWithJumpTable(left3, right3);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Params))]
    public bool Framework(ReadOnlySpan<char> left1, ReadOnlySpan<char> right1, ReadOnlySpan<char> left2, ReadOnlySpan<char> right2, ReadOnlySpan<char> left3, ReadOnlySpan<char> right3)
    {
        _dummy1 = left1.SequenceEqual(right1);
        _dummy2 = left2.SequenceEqual(right2);

        return left3.SequenceEqual(right3);
    }
}
