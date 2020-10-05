``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.546 (2004/?/20H1)
Unknown processor
.NET Core SDK=3.1.402
  [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
  Job-UIPIXW : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT

Runtime=.NET Core 3.1  Toolchain=netcoreapp31  

```
|        Method | Messages |      Mean |    Error |   StdDev | Ratio |      Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------- |--------- |----------:|---------:|---------:|------:|-----------:|------:|------:|----------:|
| **VoidValueTask** |  **1024000** |  **48.59 ms** | **0.164 ms** | **0.153 ms** |  **1.00** |  **2909.0909** |     **-** |     **-** |  **23.44 MB** |
|               |          |           |          |          |       |            |       |       |           |
| **VoidValueTask** | **10240000** | **479.07 ms** | **1.547 ms** | **1.208 ms** |  **1.00** | **29000.0000** |     **-** |     **-** | **234.38 MB** |
