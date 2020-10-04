using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Xunit.Abstractions;

namespace TestExtensions
{
    public class XunitLoggerProvider : ILoggerProvider
    {

        public XunitLoggerProvider()
        {
        }

        static XunitLoggerProvider()
        {
            CachedLogger = new ConcurrentDictionary<string, XunitLogger>();
        }

        static ConcurrentDictionary<string, XunitLogger> CachedLogger;

        static volatile ITestOutputHelper Output;
        public static void PlugOutput(ITestOutputHelper outputHelper)
        {
            Output = outputHelper;
            foreach (var logger in CachedLogger.Values)
                logger.PlugOutput(outputHelper);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return CachedLogger.GetOrAdd(categoryName, new XunitLogger(categoryName, Output));
        }


        public void Dispose()
        {
        }

        private class XunitLogger : ILogger, IDisposable
        {
            public volatile ITestOutputHelper Output;
            public string Category;
            private List<string> deferredLogs;

            public XunitLogger(string category, ITestOutputHelper output)
            {
                this.Category = category;
                this.deferredLogs = new List<string>();
                Output = output;
            }
            
            public void PlugOutput(ITestOutputHelper outputHelper)
            {
                Output = outputHelper;
                lock (deferredLogs)
                {
                    foreach (var log in deferredLogs)
                        Output.WriteLine(log ?? "null?");
                    deferredLogs.Clear();
                }
            }

            public IDisposable BeginScope<TState>(TState state) => this;

            public void Dispose() 
            {
                
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                try
                {
                    var log = $"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} => {logLevel} [{this.Category}.{eventId.Name ?? eventId.Id.ToString()}] {formatter(state, exception)} ====> {exception?.ToString()}";
                    if (Output == null)
                        lock (deferredLogs)
                            deferredLogs.Add(log);
                    else
                        Output.WriteLine(log);
                }
                catch { }
            }
        }
    }
}
