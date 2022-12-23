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

    public volatile string _str1;
    public volatile string _str1_1;

    public volatile string _str2;
    public volatile string _str2_1;

    public volatile string _str3;
    public volatile string _str3_1;

    public volatile string _str4;
    public volatile string _str4_1;

    public volatile string _str5;
    public volatile string _str5_1;

    public volatile string _str6;
    public volatile string _str6_1;

    public volatile string _str7;
    public volatile string _str7_1;

    public volatile string _str8;
    public volatile string _str8_1;

    public volatile string _str16;
    public volatile string _str16_1;

    public volatile string _str32;
    public volatile string _str32_1;

    public volatile string _str64;
    public volatile string _str64_1;

    public volatile string _str128;
    public volatile string _str128_1;

    public EqualsOrdinal()
    {
        _str1 = "x";
        _str2 = new string('x', 1) + '0';
        _str3 = new string('x', 2) + '0';
        _str4 = new string('x', 3) + '0';
        _str5 = new string('x', 4) + '0';
        _str6 = new string('x', 5) + '0';
        _str7 = new string('x', 6) + '0';
        _str8 = new string('x', 7) + '0';
        _str16 = new string('x', 15) + '0';
        _str32 = new string('x', 31) + '0';
        _str64 = new string('x', 63) + '0';
        _str128 = new string('x', 127) + '0';

        _str1_1 = "y";
        _str2_1 = new string('x', 1) + '1';
        _str3_1 = new string('x', 2) + '1';
        _str4_1 = new string('x', 3) + '1';
        _str5_1 = new string('x', 4) + '1';
        _str6_1 = new string('x', 5) + '1';
        _str7_1 = new string('x', 6) + '1';
        _str8_1 = new string('x', 7) + '1';
        _str16_1 = new string('x', 15) + '1';
        _str32_1 = new string('x', 31) + '1';
        _str64_1 = new string('x', 63) + '1';
        _str128_1 = new string('x', 127) + '1';
    }
    public IEnumerable<object[]> GetParams()
    {
        yield return new object[] { _str1, _str1_1 };
        yield return new object[] { _str2, _str2_1 };
        yield return new object[] { _str3, _str3_1 };
        yield return new object[] { _str4, _str4_1 };
        yield return new object[] { _str5, _str5_1 };
        yield return new object[] { _str6, _str6_1 };
        yield return new object[] { _str7, _str7_1 };
        yield return new object[] { _str8, _str8_1 };
        yield return new object[] { _str16, _str16_1 };
        yield return new object[] { _str32, _str32_1 };
        yield return new object[] { _str64, _str64_1 };
        yield return new object[] { _str128, _str128_1 };
    }

    #endregion

    [Benchmark]
    [ArgumentsSource(nameof(GetParams))]
    public bool EqualsOrdinal_NonConstant(ReadOnlySpan<char> str1, ReadOnlySpan<char> str2)
    {
        if (str1.Length != str2.Length) return false;

        return EqualityComparer.Equals(
            ref MemoryMarshal.GetReference(str1).AsBytePtr(),
            ref MemoryMarshal.GetReference(str2).AsBytePtr(),
            (nuint)str1.Length);
    }

    //[Benchmark]
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

    #region EqualityOperator_HalfConstant

    //[Benchmark]
    public bool EqualityOperator_HalfConstant4()
    {
        return _str4 == "xxxx";
    }

    //[Benchmark]
    public bool EqualityOperator_HalfConstant7()
    {
        return _str7 == "xxxxxxx";
    }

    //[Benchmark]
    public bool EqualityOperator_HalfConstant8()
    {
        return _str8 == "xxxxxxxx";
    }

    //[Benchmark]
    public bool EqualityOperator_HalfConstant16()
    {
        return _str16 == "xxxxxxxxxxxxxxxx";
    }

    //[Benchmark]
    public bool EqualityOperator_HalfConstant32()
    {
        return _str32 == "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
    }

    #endregion

    #region Ordinal_HalfConstant

    //[Benchmark]
    public bool Ordinal_HalfConstant4()
    {
        return string.Equals(_str4, "xxxx", System.StringComparison.Ordinal);
    }

    //[Benchmark]
    public bool Ordinal_HalfConstant7()
    {
        return string.Equals(_str7, "xxxxxxx", System.StringComparison.Ordinal);
    }

    //[Benchmark]
    public bool Ordinal_HalfConstant8()
    {
        return string.Equals(_str8, "xxxxxxxx", System.StringComparison.Ordinal);
    }

    //[Benchmark]
    public bool Ordinal_HalfConstant16()
    {
        return string.Equals(_str16, "xxxxxxxxxxxxxxxx", System.StringComparison.Ordinal);
    }

    //[Benchmark]
    public bool Ordinal_HalfConstant32()
    {
        return string.Equals(_str32, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", System.StringComparison.Ordinal);
    }

    #endregion

    #region OrdinalIgnoreCase_HalfConstant

    //[Benchmark]
    public bool OrdinalIgnoreCase_HalfConstant4()
    {
        return string.Equals(_str4, "xxxx", System.StringComparison.OrdinalIgnoreCase);
    }

    //[Benchmark]
    public bool OrdinalIgnoreCase_HalfConstant7()
    {
        return string.Equals(_str7, "xxxxxxx", System.StringComparison.OrdinalIgnoreCase);
    }

    //[Benchmark]
    public bool OrdinalIgnoreCase_HalfConstant8()
    {
        return string.Equals(_str8, "xxxxxxxx", System.StringComparison.OrdinalIgnoreCase);
    }

    //[Benchmark]
    public bool OrdinalIgnoreCase_HalfConstant16()
    {
        return string.Equals(_str16, "xxxxxxxxxxxxxxxx", System.StringComparison.OrdinalIgnoreCase);
    }

    //[Benchmark]
    public bool OrdinalIgnoreCase_HalfConstant32()
    {
        return string.Equals(_str32, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", System.StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
