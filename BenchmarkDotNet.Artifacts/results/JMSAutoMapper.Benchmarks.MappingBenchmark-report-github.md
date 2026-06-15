```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8655/25H2/2025Update/HudsonValley2)
Intel Core i5-8265U CPU 1.60GHz (Max: 1.80GHz) (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.315
  [Host]     : .NET 9.0.17 (9.0.17, 9.0.1726.26416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.17 (9.0.17, 9.0.1726.26416), X64 RyuJIT x86-64-v3


```
| Method                   | Mean         | Error        | StdDev       | Median       | Gen0    | Allocated |
|------------------------- |-------------:|-------------:|-------------:|-------------:|--------:|----------:|
| JmsMapper_SimpleMap      |    325.55 ns |    11.171 ns |    32.587 ns |    317.68 ns |  0.1478 |     464 B |
| AutoMapper_SimpleMap     |     76.91 ns |     2.679 ns |     7.857 ns |     73.71 ns |  0.0101 |      32 B |
| JmsMapper_CollectionMap  | 35,010.98 ns | 1,147.113 ns | 3,382.289 ns | 34,549.30 ns | 15.1978 |   47767 B |
| AutoMapper_CollectionMap |  1,679.98 ns |    58.306 ns |   171.917 ns |  1,693.63 ns |  1.7185 |    5392 B |
| JmsMapper_ComplexMap     |    671.54 ns |    25.181 ns |    73.455 ns |    680.53 ns |  0.2594 |     816 B |
| AutoMapper_ComplexMap    |     87.79 ns |     2.677 ns |     7.810 ns |     85.81 ns |  0.0204 |      64 B |
