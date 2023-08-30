using MyTracker.Models;

namespace MyTracker.Extensions
{
    internal static class RedditPostExtensions
    {
        public static string MostUpvotesDisplay(this RedditPostData data) => $"{data.Title} ({data.Ups})";
    }
}
