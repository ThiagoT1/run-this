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
        private IFighter _codegenProxy;
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
            _codegenProxy = _directory.AsAddress(_fighter);
        }

        public const int TenM = 10_240_000;

        [Params(TenM)]
        public int Messages;

        [Benchmark(Baseline = true, OperationsPerInvoke = 1)]
        public async Task NativeVoidTask()
        {
            int messageCount = Messages;

            for (var i = 0; i < messageCount; i++)
                await _staticProxy.GetReady();

        }

        [Benchmark(OperationsPerInvoke = 1)]
        public async Task CodeGenVoidTask()
        {
            int messageCount = Messages;

            for (var i = 0; i < messageCount; i++)
                await _codegenProxy.GetReady();

        }

        [Benchmark(OperationsPerInvoke = 1)]
        public async Task NativeValueTask()
        {
            int messageCount = Messages;

            for (var i = 0; i < messageCount; i++)
                await _staticProxy.GetRemainingHealth();

        }

        [Benchmark(OperationsPerInvoke = 1)]
        public async Task CodeGenValueTask()
        {
            int messageCount = Messages;

            for (var i = 0; i < messageCount; i++)
                await _codegenProxy.GetRemainingHealth();

        }

        [Benchmark(OperationsPerInvoke = 1)]
        public async Task NativeVoidParametersTask()
        {
            int messageCount = Messages;

            for (var i = 0; i < messageCount; i++)
                await _staticProxy.TakeDamage(10);

        }

        [Benchmark(OperationsPerInvoke = 1)]
        public async Task CodeGenVoidParametersTask()
        {
            int messageCount = Messages;

            for (var i = 0; i < messageCount; i++)
                await _codegenProxy.TakeDamage(10);

        }

        [Benchmark(OperationsPerInvoke = 1)]
        public async Task NativeValueParametersTask()
        {
            int messageCount = Messages;

            for (var i = 0; i < messageCount; i++)
                await _staticProxy.CanTakeDamage(110);

        }

        [Benchmark(OperationsPerInvoke = 1)]
        public async Task CodeGenValueParametersTask()
        {
            int messageCount = Messages;

            for (var i = 0; i < messageCount; i++)
                await _codegenProxy.CanTakeDamage(110);

        }



    }
}