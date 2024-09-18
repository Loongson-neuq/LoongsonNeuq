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
}