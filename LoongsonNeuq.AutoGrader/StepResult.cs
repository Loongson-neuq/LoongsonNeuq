namespace LoongsonNeuq.AutoGrader;

public class StepResult
{
    public double ElapsedSeconds { get; set; }

    public long? PeakWorkingSet64 { get; set; }

    public int ExitCode { get; set; }

    public int Score => Failed ? 0 : StepConfig.Score;

    public bool Failed => ExitCode != 0 || ReachedTimeout;

    public bool ReachedTimeout { get; set; }

    public string StandardOutput { get; set; } = null!;

    public string StandardError { get; set; } = null!;

    public GradingStep StepConfig { get; set; } = null!;
}