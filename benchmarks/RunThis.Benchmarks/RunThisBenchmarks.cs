using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RunThis.Core.Invoker;
using RunThis.Tests.Targets;

namespace RunThis.Benchmarks
{
    [MemoryDiagnoser]
    public class RunThisBenchmarks
    {
        private IFighter _fighter;
        private IFighter _proxy;
        public RunThisBenchmarks()
        {
        }

        [GlobalSetup]
        public void Setup()
        {
            _fighter = new Fighter();
            _proxy = new FighterProxy(_fighter, new SingleThreadInvoker());
        }

        public const int TenM = 10_240_000;

        [Params(1_024_000, TenM)]
        public int Messages;

        [Benchmark(Baseline = true, OperationsPerInvoke = 1)]
        public async Task VoidValueTask()
        {
            int messageCount = Messages;

            for (var i = 0; i < messageCount; i++)
                await _proxy.GetReady();

        }


    }
}