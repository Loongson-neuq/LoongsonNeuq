using Newtonsoft.Json;

namespace LoongsonNeuq.AutoGrader;

public class GradingStep
{
    [JsonProperty("title")]
    public string Title { get; set; } = null!;

    [JsonProperty("timeout")]
    public double Timeout { get; set; }

    [JsonProperty("command")]
    public string Command { get; set; } = null!;

    [JsonProperty("score")]
    public int Score { get; set; }  
}