using Microsoft.Extensions.Logging;

namespace LoongsonNeuq.Common;

public class Logger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => null;

    public bool IsEnabled(LogLevel logLevel)
        => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        using (var scope = ConsoleColorScope.FromLogLevel(logLevel))
        {
            Console.WriteLine($"[{DateTime.UtcNow}] [{logLevel}]: {formatter(state, exception)}");
        }
    }

    private struct ConsoleColorScope : IDisposable
    {
        private readonly ConsoleColor _previousColor;

        public static ConsoleColorScope FromLogLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => new ConsoleColorScope(ConsoleColor.Gray),
                LogLevel.Debug => new ConsoleColorScope(ConsoleColor.White),
                LogLevel.Information => new ConsoleColorScope(ConsoleColor.Green),
                LogLevel.Warning => new ConsoleColorScope(ConsoleColor.Yellow),
                LogLevel.Error => new ConsoleColorScope(ConsoleColor.Red),
                LogLevel.Critical => new ConsoleColorScope(ConsoleColor.DarkRed),
                _ => new ConsoleColorScope(ConsoleColor.Gray)
            };
        }

        private static SemaphoreSlim _singletonLock = new(1, 1);

        public ConsoleColorScope(ConsoleColor color)
        {
            _singletonLock.Wait();

            _previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
        }

        public void Dispose()
        {
            Console.ForegroundColor = _previousColor;

            _singletonLock.Release();
        }
    }
}