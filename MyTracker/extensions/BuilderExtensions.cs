using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyTracker.Interfaces;
using MyTracker.Services;
using RedditPostTracker.Interfaces;

namespace MyTracker.Extensions
{
    internal static class BuilderExtensions
    {
        public static IHostBuilder InjectServices(this IHostBuilder hostBuilder) {
            hostBuilder.ConfigureServices((context, services) =>
            {
                services
                    .AddLogging()
                    .AddHttpClient()
                    .AddSingleton<ILogger, Logger<Program>>()
                    .AddTransient<IConfigurationService, ConfigurationService>()
                    .AddTransient<IRedditTrackerService, RedditTrackerService>()
                    .AddTransient<IPostProcessor, PostProcessorService>()
                    .AddSingleton<IRedditApiClient, RedditApiClient>()
                    .AddSingleton<ITrackerLoggerService, TrackerLoggerService>();
            }).UseConsoleLifetime();

            return hostBuilder;
        }
    }
}
