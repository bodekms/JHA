namespace MyTracker.Interfaces
{
    public interface ITrackerLoggerService
    {
        void Log(string message);
        void LogError(Exception exception);
        void LogToConsole(string message);
    }
}
