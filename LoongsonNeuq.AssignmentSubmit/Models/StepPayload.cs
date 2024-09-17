using Newtonsoft.Json;

namespace LoongsonNeuq.AssignmentSubmit.Models;

public class StepPayload
{
    [JsonProperty("title")]
    public string Title { get; set; } = null!;

    [JsonProperty("elapsed_seconds")]
    public double ElapsedSeconds { get; set; }

    [JsonProperty("peak_working_set_64")]
    public long? PeakWorkingSet64 { get; set; }

    [JsonProperty("exit_code")]
    public int ExitCode { get; set; }

    [JsonProperty("score")]
    public int Score { get; set; }

    [JsonProperty("failed")]
    public bool Failed { get; set; }

    [JsonProperty("reached_timeout")]
    public bool ReachedTimeout { get; set; }
}
