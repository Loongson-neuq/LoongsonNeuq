using System.Text.Json.Serialization;

namespace LoongsonNeuq.AutoGrader;

public class GradingConfig
{
    [JsonPropertyName("enable")]
    public bool Enabled { get; set; }

    [JsonPropertyName("upload_output")]
    public bool UploadOutput { get; set; }

    [JsonPropertyName("steps")]
    public List<GradingStep> Steps { get; set; } = null!;

    public int TotalScore => Steps.Count > 0 ? Steps.Sum(step => step.Score) : 0;
}