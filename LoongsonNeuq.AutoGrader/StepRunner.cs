using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

namespace LoongsonNeuq.AutoGrader;

public class StepRunner
{
    public GradingStep Step { get; }

    public string WorkingDirectory { get; set; } = null!;

    public string Shell { get; set; } = "/bin/bash";

    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

    private ILogger? _logger { get; }

    private string commandShPath = null!;

    public StepRunner(GradingStep step, ILogger? logger = null)
    {
        Step = step;
        _logger = logger;
    }

    private void setupCommandSh()
    {
        commandShPath = Path.GetTempFileName();
        File.WriteAllText(commandShPath, Step.Command);

        _logger?.LogInformation($"  Command file created: {commandShPath}");
    }

    private void setupWorkingDirectory()
    {
        if (string.IsNullOrEmpty(WorkingDirectory))
        {
            WorkingDirectory = Environment.CurrentDirectory;

            _logger?.LogInformation($"  Working directory not set, using Runner's working directory");
        }

        _logger?.LogInformation($"  Working directory: {WorkingDirectory}");
    }

    public async Task<StepResult> RunAsync()
    {
        _logger?.LogInformation($"Start running step: {Step.Title}");
        _logger?.LogInformation($"    Score   : {Step.Score}");
        _logger?.LogInformation($"    Command : {Step.Command}");
        _logger?.LogInformation($"    Timeout : {Step.Timeout}");
        _logger?.LogInformation($"    Shell   : {Shell}");

        setupCommandSh();

        setupWorkingDirectory();

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Shell,
                Arguments = commandShPath,
                WorkingDirectory = WorkingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        _logger?.LogInformation("  Start running process.");

        var runContext = new RunProcessContext(process, Step, CancellationToken);

        StepResult result = await runContext.RunAsync();

        _logger?.LogInformation($"  Step run finished with result: ");
        _logger?.LogInformation($"    Elapsed  : {result.ElapsedSeconds:F4}s");
        _logger?.LogInformation($"    ExitCode : {result.ExitCode}");
        _logger?.LogInformation($"    PeakMem  : {result.PeakWorkingSet64 ?? 0} bytes");
        _logger?.LogInformation($"    LTE      : {result.ReachedTimeout}");
        _logger?.LogInformation($"    Failed   : {result.Failed}");
        _logger?.LogInformation($"    Score    : {result.Score}");

        if (result.PeakWorkingSet64 is null)
        {
            _logger?.LogWarning("The process exited too quickly to capture peak memory usage. The value is not available.");
        }

        return result;
    }
}