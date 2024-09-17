using LoongsonNeuq.AutoGrader;
using Newtonsoft.Json;

namespace LoongsonNeuq.AssignmentSubmit.Configuration;

/// <summary>
/// Json object model for assignment configuration
/// </summary>
public class AssignmentConfig
{
    [JsonProperty("assignment_name")]
    public string Name { get; set; } = null!;

    [JsonProperty("description")]
    public string Description { get; set; } = null!;

    [JsonProperty("type")]
    public string AssignmentType { get; set; } = null!;

    [JsonProperty("status")]
    public string Status { get; set; } = null!;

    [JsonProperty("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("version")]
    public int Version { get; set; }

    [JsonProperty("auto_grade")]
    public GradingConfig AutoGrade { get; set; } = null!;

    public string CategorySpecificAssignmentId =>
        $"{NormalizedType}-{Id}";

    private string NormalizedType
        => AssignmentType.Trim().ToUpper() is { } value && value is "OS" or "CPU"
            ? value
            : "UNKNOWN";
}