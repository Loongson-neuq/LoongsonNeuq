using System.Text.Json.Serialization;

namespace LoongsonNeuq.AutoGrader;

public class GradingStep
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("timeout")]
    public double Timeout { get; set; }

    [JsonPropertyName("command")]
    public string Command { get; set; } = null!;

    [JsonPropertyName("score")]
    public int Score { get; set; }

    /// <summary>
    /// Whether the step is required to pass.
    /// If enabled and some steps are failed, the CI will fail.
    /// </summary>
    [JsonPropertyName("required")]
    public bool? Required { get; set; }
}