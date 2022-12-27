using BenchmarkDotNet.Running;
using Ofella.Utilities.Memory.Benchmark.Scenarios;
using Ofella.Utilities.Memory.Benchmarks.ReadOnlySpanOfChar;
using Ofella.Utilities.Memory.Comparing;
using System.Diagnostics;
using System.Numerics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

//BenchmarkRunner.Run(typeof(Copying));
BenchmarkRunner.Run(typeof(EqualsOrdinal));

//var testClass = new EqualsOrdinal();
//var left = (new string('x', 12) + '1').AsSpan();
//var right = (new string('x', 12) + '2').AsSpan();

//for (var i = 0; i < 1_000_000; ++i)
//{
//    left.EqualsOrdinal(right);
//}

//Console.ReadKey();
//Console.WriteLine(Marshal.GetLastWin32Error());

//left.EqualsOrdinal(right);
