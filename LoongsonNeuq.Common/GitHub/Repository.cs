using System.Text.Json.Serialization;

namespace LoongsonNeuq.Common.GitHub;

public class Repository
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = null!;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = null!;

    [JsonPropertyName("private")]
    public bool Private { get; set; }

    [JsonPropertyName("default_branch")]
    public string DefaultBranch { get; set; } = null!;
}