```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8655/25H2/2025Update/HudsonValley2)
Intel Core i5-8265U CPU 1.60GHz (Max: 1.80GHz) (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.315
  [Host]     : .NET 9.0.17 (9.0.17, 9.0.1726.26416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.17 (9.0.17, 9.0.1726.26416), X64 RyuJIT x86-64-v3


```
| Method                   | Mean         | Error        | StdDev       | Gen0    | Allocated |
|------------------------- |-------------:|-------------:|-------------:|--------:|----------:|
| JmsMapper_SimpleMap      |    333.34 ns |     9.718 ns |    28.500 ns |  0.1478 |     464 B |
| AutoMapper_SimpleMap     |     79.72 ns |     2.440 ns |     7.156 ns |  0.0101 |      32 B |
| JmsMapper_CollectionMap  | 35,195.92 ns | 1,105.085 ns | 3,258.368 ns | 15.1978 |   47767 B |
| AutoMapper_CollectionMap |  1,655.31 ns |    61.837 ns |   181.358 ns |  1.7185 |    5392 B |
| JmsMapper_ComplexMap     |    707.02 ns |    21.407 ns |    61.075 ns |  0.2594 |     816 B |
| AutoMapper_ComplexMap    |     91.50 ns |     3.704 ns |    10.627 ns |  0.0204 |      64 B |
