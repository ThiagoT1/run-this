using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace RunThis.Benchmarks
{
    class Program
    {
        public static async Task Main(string[] args)
        {

            await Task.Delay(1);


#if DEBUG

            Stopwatch watch = Stopwatch.StartNew();

            //Debug Only
            var bench = new RunThisBenchmarks();

            bench.Messages = RunThisBenchmarks.TenM;
            bench.Setup();
            await bench.StaticVoidValueTask();
            await bench.MetaVoidValueTask();


            Console.WriteLine("");
            Console.WriteLine($"HOT ({RunThisBenchmarks.TenM:N0} messages) => ");
            Console.WriteLine($"{((RunThisBenchmarks.TenM / watch.Elapsed.TotalMilliseconds) * 1000.0):N2} msg/s");
            Console.WriteLine("");

#else
            BenchmarkSwitcher.FromAssemblies(new[] { typeof(Program).Assembly }).Run(args);
#endif



        }
    }
}
