namespace MyTracker.Interfaces
{
    public interface IRedditTrackerService
    {
        Task TrackNewPostsInSubRedditsAsync(List<string> subReddit, CancellationToken cancellationToken);
    }
}
