using Microsoft.Extensions.Logging;

namespace LoongsonNeuq.Common;

public class DummyLogger : ILogger
{
    public static readonly DummyLogger Instance = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => null;

    public bool IsEnabled(LogLevel logLevel)
        => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // ignore
    }
}
