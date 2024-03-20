using Microsoft.Extensions.Logging;
using System.Text;

namespace UsbWatcherUi.Utils
{
    public class StringAppendLogger : ILogger
    {
        private readonly StringBuilder log = new();

        public event Action<StringBuilder, string>? LogUpdated;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string newLogLine = $"[{DateTime.Now:u}] [{logLevel}] {formatter(state, exception)}";
            log.AppendLine(newLogLine);
            LogUpdated?.Invoke(log, newLogLine);
        }
    }
}
