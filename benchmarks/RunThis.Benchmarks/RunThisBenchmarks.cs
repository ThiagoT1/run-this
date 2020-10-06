using System.Reflection.Metadata;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RunThis.Core.Directory;
using RunThis.Core.Invoker;
using RunThis.Tests.Targets;

namespace RunThis.Benchmarks
{
    [MemoryDiagnoser]
    public class RunThisBenchmarks
    {
        private IFighter _fighter;
        private IFighter _staticProxy;
        private IFighter _metaProxy;
        private IInvokerDirectory _directory;
        public RunThisBenchmarks()
        {
        }

        [GlobalSetup]
        public void Setup()
        {
            _directory = new InvokerDirectory(null);
            _fighter = new Fighter();
            _staticProxy = new FighterProxy(_fighter, new SingleThreadInvoker());
            _metaProxy = _directory.AsAddress(_fighter);
        }

        public const int TenM = 10_240_000;

        [Params(1_024_000, TenM)]
        public int Messages;

        [Benchmark(Baseline = true, OperationsPerInvoke = 1)]
        public async Task StaticVoidValueTask()
        {
            int messageCount = Messages;

            for (var i = 0; i < messageCount; i++)
                await _staticProxy.GetReady();

        }

        [Benchmark(OperationsPerInvoke = 1)]
        public async Task MetaVoidValueTask()
        {
            int messageCount = Messages;

            for (var i = 0; i < messageCount; i++)
                await _metaProxy.GetReady();

        }


    }
}