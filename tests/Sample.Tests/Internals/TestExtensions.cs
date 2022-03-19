namespace Sample.Tests
{
    using System;
    using MassTransit;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;


    public static class TestExtensions
    {
        public static void ConfigureLogging(this IServiceProvider provider)
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

            LogContext.ConfigureCurrentLogContext(loggerFactory);
        }
    }
}