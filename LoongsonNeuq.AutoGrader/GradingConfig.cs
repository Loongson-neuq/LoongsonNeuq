using Newtonsoft.Json;

namespace LoongsonNeuq.AutoGrader;

public class GradingConfig
{
    [JsonProperty("enable")]
    public bool Enabled { get; set; }

    [JsonProperty("upload_output")]
    public bool UploadOutput { get; set; }

    [JsonProperty("steps")]
    public List<GradingStep> Steps { get; set; } = null!;

    public int TotalScore => Steps.Count > 0 ? Steps.Sum(step => step.Score) : 0;
}