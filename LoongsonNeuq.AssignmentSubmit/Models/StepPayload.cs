using System.Text.Json.Serialization;
using LoongsonNeuq.AutoGrader;

namespace LoongsonNeuq.AssignmentSubmit.Models;

public class StepPayload
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("elapsed_seconds")]
    public double ElapsedSeconds { get; set; }

    [JsonPropertyName("peak_working_set_64")]
    public long? PeakWorkingSet64 { get; set; }

    [JsonPropertyName("exit_code")]
    public int ExitCode { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("full_score")]
    public int FullScore => StepConfig.Score;

    [JsonPropertyName("failed")]
    public bool Failed { get; set; }

    [JsonPropertyName("reached_timeout")]
    public bool ReachedTimeout { get; set; }

    [JsonPropertyName("output_folder")]
    public string? OutputFolder { get; set; } = null;

    [JsonPropertyName("stdout_file")]
    public string? StandardOutputFile { get; set; } = null;

    [JsonPropertyName("stderr_file")]
    public string? StandardErrorFile { get; set; } = null;

    [JsonIgnore]
    public StepResult StepResult { get; set; } = null!;

    [JsonIgnore]
    public GradingStep StepConfig { get; set; } = null!;
}
