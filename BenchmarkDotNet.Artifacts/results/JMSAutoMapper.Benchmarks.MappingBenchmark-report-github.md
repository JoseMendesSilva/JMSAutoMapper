```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8655/25H2/2025Update/HudsonValley2)
Intel Core i5-8265U CPU 1.60GHz (Max: 1.80GHz) (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.315
  [Host]     : .NET 9.0.17 (9.0.17, 9.0.1726.26416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.17 (9.0.17, 9.0.1726.26416), X64 RyuJIT x86-64-v3


```
| Method                   | Mean        | Error       | StdDev      | Median       | Gen0    | Allocated |
|------------------------- |------------:|------------:|------------:|-------------:|--------:|----------:|
| JmsMapper_SimpleMap      |    409.1 ns |    21.81 ns |    63.62 ns |    396.47 ns |  0.1478 |     464 B |
| AutoMapper_SimpleMap     |    101.3 ns |     5.43 ns |    15.84 ns |     95.93 ns |  0.0101 |      32 B |
| JmsMapper_CollectionMap  | 47,270.7 ns | 2,434.27 ns | 7,062.27 ns | 45,758.51 ns | 15.1367 |   47542 B |
| AutoMapper_CollectionMap |  2,177.0 ns |   122.47 ns |   355.30 ns |  2,114.81 ns |  1.7166 |    5392 B |
| JmsMapper_ComplexMap     |    786.7 ns |    40.34 ns |   118.32 ns |    761.91 ns |  0.2594 |     816 B |
| AutoMapper_ComplexMap    |    106.3 ns |     5.70 ns |    16.35 ns |    102.28 ns |  0.0203 |      64 B |
