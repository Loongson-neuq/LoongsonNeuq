using Newtonsoft.Json;

namespace LoonsonNeuq.AssignmentSubmit.Configuration;

/// <summary>
/// Json object model for auto grade configuration
/// </summary>
public class AutoGradeConfig
{
    [JsonProperty("enable")]
    public bool Enabled { get; set; }

    /// <summary>
    /// The timeout in seconds.
    /// </summary>
    [JsonProperty("timeout")]
    public int Timeout { get; set; }

    [JsonProperty("command")]
    public string Command { get; set; } = null!;

    [JsonProperty("max_score")]
    public int MaxScore { get; set; }

    [JsonProperty("upload_output")]
    public bool UploadOutput { get; set; }
}