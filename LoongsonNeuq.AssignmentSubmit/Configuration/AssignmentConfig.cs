using System.Text.Json.Serialization;
using LoongsonNeuq.AutoGrader;

namespace LoongsonNeuq.AssignmentSubmit.Configuration;

/// <summary>
/// Json object model for assignment configuration
/// </summary>
public class AssignmentConfig
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("type")]
    public string AssignmentType { get; set; } = null!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = null!;

    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("auto_grade")]
    public GradingConfig AutoGrade { get; set; } = null!;

    public string CategorySpecificAssignmentId =>
        $"{NormalizedType}-{Id}";

    private string NormalizedType
        => AssignmentType.Trim().ToUpper() is { } value && value is "OS" or "CPU"
            ? value
            : "UNKNOWN";
}