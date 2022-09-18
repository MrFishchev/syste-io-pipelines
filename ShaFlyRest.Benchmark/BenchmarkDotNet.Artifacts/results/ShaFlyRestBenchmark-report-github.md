``` ini

BenchmarkDotNet=v0.13.2, OS=macOS Monterey 12.4 (21F79) [Darwin 21.5.0]
Intel Core i9-9880H CPU 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.202
  [Host]     : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT AVX2
  Job-WRXJRE : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT AVX2

IterationCount=1  LaunchCount=1  RunStrategy=Monitoring  
WarmupCount=0  

```
|                                     Method |    Mean | Error |      Gen0 |      Gen1 |      Gen2 | Allocated |
|------------------------------------------- |--------:|------:|----------:|----------:|----------:|----------:|
|                             SendUsingPipes | 45.07 s |    NA |         - |         - |         - |   1.92 MB |
|                            SendUsingStream | 45.26 s |    NA |         - |         - |         - |   1.86 MB |
|                   SendAndProcessUsingPipes | 46.45 s |    NA |         - |         - |         - |   3.66 MB |
|                  SendAndProcessUsingStream | 46.99 s |    NA | 2000.0000 | 2000.0000 | 2000.0000 |  73.45 MB |
|          SendAndProcessUsingOptimizedPipes | 45.16 s |    NA |         - |         - |         - |   3.28 MB |
|     SendAndProcessUsingOptimizedPipesAsync | 45.25 s |    NA |         - |         - |         - |   3.93 MB |
| SendAndProcessUsingOptimizedPipesBigBuffer | 46.31 s |    NA |         - |         - |         - |   2.84 MB |
