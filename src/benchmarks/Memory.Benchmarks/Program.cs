using BenchmarkDotNet.Running;
using Ofella.Utilities.Memory.Benchmark.Scenarios;
using Ofella.Utilities.Memory.Benchmarks.Comparing;
using Ofella.Utilities.Memory.Comparing;
using System.Diagnostics;
using System.Numerics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

//BenchmarkRunner.Run(typeof(Copying));
BenchmarkRunner.Run(typeof(OptimizedSequenceEqual));

//var left = (new string('x', 3) + '1').AsSpan();
//var right = (new string('x', 3) + '2').AsSpan();

//for (var i = 0; i < 1_000_000; ++i)
//{
//    Comparer.SequenceEqualWithMoreBranches(left, right);
//    //left.SequenceEqual(right);
//}

//Console.ReadKey();
//Console.WriteLine(Marshal.GetLastWin32Error());

//Comparer.SequenceEqualWithMoreBranches(left, right);
////left.SequenceEqual(right);