``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
Unknown processor
.NET Core SDK=3.1.402
  [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
  Job-VBGJOW : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT

Runtime=.NET Core 3.1  Toolchain=netcoreapp31  

```
|              Method | Messages |      Mean |    Error |   StdDev | Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------- |--------- |----------:|---------:|---------:|------:|--------:|-----------:|------:|------:|----------:|
| **StaticVoidValueTask** |  **1024000** |  **50.27 ms** | **0.481 ms** | **0.450 ms** |  **1.00** |    **0.00** |  **2909.0909** |     **-** |     **-** |  **23.44 MB** |
|   MetaVoidValueTask |  1024000 |  51.05 ms | 1.223 ms | 1.085 ms |  1.02 |    0.02 |  2900.0000 |     - |     - |  23.44 MB |
|                     |          |           |          |          |       |         |            |       |       |           |
| **StaticVoidValueTask** | **10240000** | **491.46 ms** | **2.097 ms** | **1.962 ms** |  **1.00** |    **0.00** | **29000.0000** |     **-** |     **-** | **234.38 MB** |
|   MetaVoidValueTask | 10240000 | 531.35 ms | 1.460 ms | 1.365 ms |  1.08 |    0.01 | 29000.0000 |     - |     - | 234.38 MB |
