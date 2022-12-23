using BenchmarkDotNet.Running;
using Ofella.Utilities.Memory.Benchmark.Scenarios;
using Ofella.Utilities.Memory.Benchmarks.ReadOnlySpanOfChar;

//BenchmarkRunner.Run(typeof(Copying));
BenchmarkRunner.Run(typeof(EqualsOrdinal));

//var testClass = new EqualsOrdinal();

//for (var i = 0; i < 100_000; ++i)
//{
//    testClass.EqualityOperator_NonConstant(new string('x', 3) + '1', new string('x', 3) + '2');
//}

//testClass.EqualityOperator_NonConstant(new string('x', 3) + '1', new string('x', 3) + '2');
