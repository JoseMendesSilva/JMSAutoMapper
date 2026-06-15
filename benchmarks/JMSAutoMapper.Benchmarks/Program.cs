using BenchmarkDotNet.Running;
using JMSAutoMapper.Benchmarks;

var summary = BenchmarkRunner.Run<MappingBenchmark>();