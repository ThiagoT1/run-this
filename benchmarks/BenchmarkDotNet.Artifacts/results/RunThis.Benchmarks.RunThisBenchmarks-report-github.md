``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
Unknown processor
.NET Core SDK=3.1.402
  [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
  Job-YMVYNC : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT

Runtime=.NET Core 3.1  Toolchain=netcoreapp31  

```
|                     Method | Messages |     Mean |   Error |   StdDev |   Median | Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------------- |--------- |---------:|--------:|---------:|---------:|------:|--------:|-----------:|------:|------:|----------:|
|             NativeVoidTask | 10240000 | 495.2 ms | 9.72 ms | 15.41 ms | 487.0 ms |  1.00 |    0.00 | 29000.0000 |     - |     - | 234.38 MB |
|            CodeGenVoidTask | 10240000 | 489.3 ms | 2.47 ms |  2.19 ms | 489.0 ms |  0.99 |    0.02 | 29000.0000 |     - |     - | 234.38 MB |
|            NativeValueTask | 10240000 | 543.2 ms | 1.98 ms |  1.75 ms | 542.9 ms |  1.10 |    0.03 | 29000.0000 |     - |     - | 234.38 MB |
|           CodeGenValueTask | 10240000 | 500.3 ms | 4.19 ms |  3.92 ms | 500.4 ms |  1.01 |    0.02 | 29000.0000 |     - |     - | 234.38 MB |
|   NativeVoidParametersTask | 10240000 | 509.6 ms | 8.96 ms |  8.39 ms | 506.7 ms |  1.03 |    0.03 | 39000.0000 |     - |     - |  312.5 MB |
|  CodeGenVoidParametersTask | 10240000 | 494.7 ms | 2.80 ms |  2.34 ms | 495.1 ms |  1.00 |    0.02 | 39000.0000 |     - |     - |  312.5 MB |
|  NativeValueParametersTask | 10240000 | 542.7 ms | 1.41 ms |  1.25 ms | 543.0 ms |  1.10 |    0.03 | 39000.0000 |     - |     - |  312.5 MB |
| CodeGenValueParametersTask | 10240000 | 541.2 ms | 4.05 ms |  3.79 ms | 542.1 ms |  1.10 |    0.03 | 39000.0000 |     - |     - |  312.5 MB |
