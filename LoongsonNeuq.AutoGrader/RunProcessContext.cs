using System.Diagnostics;
using System.Text;

namespace LoongsonNeuq.AutoGrader;

public class RunProcessContext
{
    private Process _process;
    private GradingStep _step;

    private CancellationToken _cancellationToken;

    public RunProcessContext(Process process, GradingStep step, CancellationToken cancellationToken)
    {
        _process = process;
        _step = step;
        _cancellationToken = cancellationToken;
    }

    private long? caputredPeakWorkingSet64 = null;

    private async Task MemoryMonitorAsync(CancellationToken cancellationToken)
    {
        while (!_process.HasExited && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                caputredPeakWorkingSet64 = _process.PeakWorkingSet64;
            }
            finally
            {
                await Task.Yield();
            }
        }
    }

    private StringBuilder standardOutput = new StringBuilder();
    private StringBuilder standardError = new StringBuilder();

    private void onReceivedStandardOutput(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is not null)
        {
            standardOutput.AppendLine(e.Data);
        }
    }

    private void onReceivedStandardError(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is not null)
        {
            standardError.AppendLine(e.Data);
        }
    }

    private async Task TimeoutMonitorAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(_step.Timeout), cancellationToken);
    }

    public async Task<StepResult> RunAsync()
    {
        _process.OutputDataReceived += onReceivedStandardOutput;
        _process.ErrorDataReceived += onReceivedStandardError;

        var stopwatch = new Stopwatch();

        bool reachedTimeout = false;
        Task firstExitedTask;

        _process.Start();
        using (var timeMeasuredScope = new TimeMeasuredScope(stopwatch))
        {
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            Task timeoutTask;

            var tasks = new List<Task>
            {
                _process.WaitForExitAsync(_cancellationToken),
                MemoryMonitorAsync(_cancellationToken),
                (timeoutTask = TimeoutMonitorAsync(_cancellationToken)),
            };

            firstExitedTask = await Task.WhenAny(tasks);

            if (firstExitedTask is not null && firstExitedTask == timeoutTask)
            {
                reachedTimeout = true;

                _process.OutputDataReceived -= onReceivedStandardOutput;
                _process.ErrorDataReceived -= onReceivedStandardError;

                _process.Kill(true);

                await _process.WaitForExitAsync(_cancellationToken);
            }
        }

        return new StepResult
        {
            ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
            PeakWorkingSet64 = caputredPeakWorkingSet64,
            ExitCode = _process.ExitCode,
            StandardOutput = standardOutput.ToString(),
            StandardError = standardError.ToString(),
            StepConfig = _step,
            ReachedTimeout = reachedTimeout,
        };
    }

    private struct TimeMeasuredScope : IDisposable
    {
        private readonly Stopwatch _stopwatch = null!;

        public readonly TimeSpan Elapsed => _stopwatch.Elapsed;

        public TimeMeasuredScope()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public TimeMeasuredScope(Stopwatch stopwatch)
        {
            _stopwatch = stopwatch;
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
        }
    }
}