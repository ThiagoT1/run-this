using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestExtensions;

namespace RunThis.Tests
{
    public class RunThisTestFixture
    {
        public readonly ServiceProvider Provider;

        public RunThisTestFixture()
        {
            Provider = new ServiceCollection()
            .AddLogging((logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                logging.AddEventSourceLogger();
                logging.AddProvider(new XunitLoggerProvider());
            })
            .BuildServiceProvider();
        }
    }
}