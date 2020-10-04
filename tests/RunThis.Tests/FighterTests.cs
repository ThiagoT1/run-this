using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestExtensions;
using Xunit;
using Xunit.Abstractions;

namespace RunThis.Tests
{



    public class FighterTests : IClassFixture<RunThisTestFixture>
    {
        private readonly RunThisTestFixture _fixture;
        private readonly ITestOutputHelper _output;

        public FighterTests(ITestOutputHelper output, RunThisTestFixture fixture)
        {
            _fixture = fixture;
            _output = output;
            XunitLoggerProvider.PlugOutput(output);
        }

        [Fact]
        public async Task GetReady()
        {
            IFighter fighter = new Fighter();
            IFighter proxy = new FighterProxy(fighter, _fixture.Provider.GetRequiredService<ILoggerFactory>());
            await proxy.GetReady();
        }

        [Theory]
        [InlineData(10_240_000)]
        public async Task Throughput_GetReady(int messageCount)
        {
            IFighter fighter = new Fighter();
            IFighter proxy = new FighterProxy(fighter, _fixture.Provider.GetRequiredService<ILoggerFactory>());

            Stopwatch watch = Stopwatch.StartNew();
            watch.Reset();

            await proxy.GetReady();

            watch.Stop();

            _output.WriteLine("COLD (1 message to warm it up) => ");
            _output.WriteLine($"{((1 / watch.Elapsed.TotalMilliseconds) * 1000.0):0.000} msg/s");
            _output.WriteLine("");

            watch.Restart();

            for (var i = 0; i < messageCount; i++)
                await proxy.GetReady();

            watch.Stop();

            _output.WriteLine("");
            _output.WriteLine($"HOT ({messageCount:N0} messages) => ");
            _output.WriteLine($"{((messageCount / watch.Elapsed.TotalMilliseconds) * 1000.0):N2} msg/s");
            _output.WriteLine("");

        }

        [Theory]
        [InlineData(10_240_000)]
        public async Task Throughput_GetRemainingHealth(int messageCount)
        {
            IFighter fighter = new Fighter();
            IFighter proxy = new FighterProxy(fighter, _fixture.Provider.GetRequiredService<ILoggerFactory>());

            Stopwatch watch = Stopwatch.StartNew();
            watch.Reset();

            await proxy.GetRemainingHealth();

            watch.Stop();

            _output.WriteLine("COLD (1 message to warm it up) => ");
            _output.WriteLine($"{((1 / watch.Elapsed.TotalMilliseconds) * 1000.0):0.000} msg/s");
            _output.WriteLine("");

            watch.Restart();

            for (var i = 0; i < messageCount; i++)
                await proxy.GetRemainingHealth();

            watch.Stop();

            _output.WriteLine("");
            _output.WriteLine($"HOT ({messageCount:N0} messages) => ");
            _output.WriteLine($"{((messageCount / watch.Elapsed.TotalMilliseconds) * 1000.0):N2} msg/s");
            _output.WriteLine("");

        }
    }
}
