using BenchmarkDotNet.Running;
using Ofella.Utilities.Memory.Benchmark.Defragmentation;
using Ofella.Utilities.Memory.Benchmarks.Comparing;
using Ofella.Utilities.Memory.Benchmarks.Reading;
using Ofella.Utilities.Memory.Comparing;
using System.Runtime.InteropServices;

//BenchmarkRunner.Run(typeof(Streaming));
//BenchmarkRunner.Run(typeof(Copying));
//BenchmarkRunner.Run(typeof(OptimizedSequenceEqualForDifferentLengths));

BenchmarkRunner.Run<Enumeration>();

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