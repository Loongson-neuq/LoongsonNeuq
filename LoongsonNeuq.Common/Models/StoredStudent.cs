using System.Text.Json.Serialization;

namespace LoongsonNeuq.Common.Models;

public class StoredStudent
{
    [JsonPropertyName("github_id")]
    public string GitHubId { get; set; } = null!;

    [JsonPropertyName("research_focus")]
    public List<string> ResearchFocus { get; set; } = null!;
}