using System.Text.Json.Serialization;

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

    [JsonPropertyName("failed")]
    public bool Failed { get; set; }

    [JsonPropertyName("reached_timeout")]
    public bool ReachedTimeout { get; set; }
}
