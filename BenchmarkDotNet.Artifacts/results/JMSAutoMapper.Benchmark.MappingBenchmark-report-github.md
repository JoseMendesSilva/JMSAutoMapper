```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8655/25H2/2025Update/HudsonValley2)
Intel Core i5-8265U CPU 1.60GHz (Max: 1.80GHz) (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.315
  [Host]     : .NET 9.0.17 (9.0.17, 9.0.1726.26416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.17 (9.0.17, 9.0.1726.26416), X64 RyuJIT x86-64-v3


```
| Method                   | Mean         | Error        | StdDev       | Gen0    | Allocated |
|------------------------- |-------------:|-------------:|-------------:|--------:|----------:|
| JmsMapper_SimpleMap      |    327.59 ns |     8.978 ns |    26.188 ns |  0.1478 |     464 B |
| AutoMapper_SimpleMap     |     78.77 ns |     2.719 ns |     8.018 ns |  0.0101 |      32 B |
| JmsMapper_CollectionMap  | 38,612.72 ns | 2,231.092 ns | 6,437.212 ns | 15.1367 |   47542 B |
| AutoMapper_CollectionMap |  1,736.49 ns |    65.461 ns |   193.013 ns |  1.7185 |    5392 B |
| JmsMapper_ComplexMap     |    593.90 ns |    13.786 ns |    39.996 ns |  0.2594 |     816 B |
| AutoMapper_ComplexMap    |     84.87 ns |     2.169 ns |     6.361 ns |  0.0204 |      64 B |
