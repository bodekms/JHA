using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyTracker.Extensions;
using MyTracker.Services;

namespace MyTracker
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Welcome();

            var builder = new HostBuilder()
                .InjectServices();          

            await Run(builder);
        }

        private static void Welcome()
        {
            Console.WriteLine("****************************************");
            Console.WriteLine("* This will poll the Reddit API to get posts on specified subreddits");
            Console.WriteLine("* Any time if finds new posts, it will log it to the console");
            Console.WriteLine("* After a set amount of requests, it will display stats.  The number of requests is configurable.");
            Console.WriteLine("****************************************");
            Console.WriteLine();
            Console.WriteLine("Press [enter] to start...");
            Console.ReadLine();
        }

        private static async Task Run(IHostBuilder builder)
        {
            var host = builder.Build();

            using var serviceScope = host.Services.CreateScope();
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, evtArgs) =>
            {
                cancellationTokenSource.Cancel();
                evtArgs.Cancel = true;
            };

            var services = serviceScope.ServiceProvider;
            var loggerService = services.GetRequiredService<TrackerLoggerService>();
            try
            {
                var trackerService = services.GetRequiredService<RedditTrackerService>();
                List<string> subReddits = new() { "askreddit" };

                await trackerService.TrackNewPostsInSubRedditsAsync(subReddits, cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                loggerService.LogError(ex);
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}