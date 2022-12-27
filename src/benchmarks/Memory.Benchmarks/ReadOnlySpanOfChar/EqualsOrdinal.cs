using BenchmarkDotNet.Attributes;
using Ofella.Utilities.Memory.Comparing;
using Ofella.Utilities.Memory.ManagedPointers;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Ofella.Utilities.Memory.Benchmarks.ReadOnlySpanOfChar;

[MemoryDiagnoser]
public class EqualsOrdinal
{
    #region Benchmark Config

    public static readonly int[] Sizes = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 32, 64, 128, 256 };

    public string[] _strLeft;
    public string[] _strRight;

    public EqualsOrdinal()
    {
        _strLeft = new string[Sizes.Length];
        _strRight = new string[Sizes.Length];

        _strLeft[0] = "0";
        _strRight[0] = "1";

        for (var i = 1; i < Sizes.Length; ++i)
        {
            _strLeft[i] = new string('x', Sizes[i] - 1) + '0';
            _strRight[i] = new string('x', Sizes[i] - 1) + '1';
        }
    }
    public IEnumerable<object[]> GetParams()
    {
        //yield return new object[] { _strLeft[0], _strRight[0] };
        //yield return new object[] { _strLeft[1], _strRight[1] };
        //yield return new object[] { _strLeft[2], _strRight[2] };
        //yield return new object[] { _strLeft[3], _strRight[3] };
        //yield return new object[] { _strLeft[4], _strRight[4] };
        yield return new object[] { _strLeft[5], _strRight[5] };
        //yield return new object[] { _strLeft[6], _strRight[6] };
        //yield return new object[] { _strLeft[7], _strRight[7] };
        //yield return new object[] { _strLeft[8], _strRight[8] };
        //yield return new object[] { _strLeft[9], _strRight[9] };
        //yield return new object[] { _strLeft[10], _strRight[10] };
        //yield return new object[] { _strLeft[11], _strRight[11] };
        //yield return new object[] { _strLeft[12], _strRight[12] };
        //yield return new object[] { _strLeft[13], _strRight[13] };
        //yield return new object[] { _strLeft[14], _strRight[14] };
        //yield return new object[] { _strLeft[15], _strRight[15] };
        //yield return new object[] { _strLeft[16], _strRight[16] };
    }

    #endregion

    [Benchmark]
    [ArgumentsSource(nameof(GetParams))]
    public bool EqualsOrdinal_NonConstant2(ReadOnlySpan<char> str1, ReadOnlySpan<char> str2)
    {
        return str1.EqualsOrdinal(str2);
    }

    [Benchmark]
    [ArgumentsSource(nameof(GetParams))]
    public bool EqualityOperator_NonConstant(string str1, string str2)
    {
        return str1 == str2;
    }

    //[Benchmark]
    [ArgumentsSource(nameof(GetParams))]
    public bool Ordinal_NonConstant(string str1, string str2)
    {
        return string.Equals(str1, str2, StringComparison.Ordinal);
    }

    //[Benchmark]
    [ArgumentsSource(nameof(GetParams))]
    public bool OrdinalIgnoreCase_NonConstant(string str1, string str2)
    {
        return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
    }
}
