using BenchmarkDotNet.Running;
using JMSAutoMapper.Benchmark;

var summary = BenchmarkRunner.Run<MappingBenchmark>();