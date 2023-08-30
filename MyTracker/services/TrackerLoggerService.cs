using Microsoft.Extensions.Logging;
using MyTracker.Interfaces;

namespace MyTracker.Services
{
    public class TrackerLoggerService : ITrackerLoggerService
    {
        private readonly ILogger _logger;

        public TrackerLoggerService(ILogger logger) 
        { 
            _logger = logger;
        }

        public void Log(string message)
        {
            _logger.Log(LogLevel.Information, message);
            LogToConsole($"Log: {message}");
        }

        public void LogError(Exception exception)
        {
            _logger.LogError(exception.Message, exception);
            LogToConsole($"Error: {exception.Message}");
        }

        public void LogToConsole(string message)
        {
            Console.WriteLine(message);
        }
    }
}
