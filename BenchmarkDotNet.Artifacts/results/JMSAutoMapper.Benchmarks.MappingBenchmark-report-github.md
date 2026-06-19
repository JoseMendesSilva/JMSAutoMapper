```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8655/25H2/2025Update/HudsonValley2)
Intel Core i5-8265U CPU 1.60GHz (Max: 1.80GHz) (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.315
  [Host]     : .NET 9.0.17 (9.0.17, 9.0.1726.26416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.17 (9.0.17, 9.0.1726.26416), X64 RyuJIT x86-64-v3


```
| Method                   | Mean         | Error      | StdDev       | Gen0    | Allocated |
|------------------------- |-------------:|-----------:|-------------:|--------:|----------:|
| JmsMapper_SimpleMap      |    355.37 ns |  10.705 ns |    31.227 ns |  0.1478 |     464 B |
| AutoMapper_SimpleMap     |     83.94 ns |   1.746 ns |     4.443 ns |  0.0101 |      32 B |
| JmsMapper_CollectionMap  | 38,304.55 ns | 985.734 ns | 2,812.356 ns | 15.1367 |   47542 B |
| AutoMapper_CollectionMap |  1,825.46 ns |  35.210 ns |    89.621 ns |  1.7185 |    5392 B |
| JmsMapper_ComplexMap     |    679.82 ns |  13.647 ns |    38.937 ns |  0.2594 |     816 B |
| AutoMapper_ComplexMap    |     92.34 ns |   1.920 ns |     5.508 ns |  0.0204 |      64 B |
