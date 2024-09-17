using Newtonsoft.Json;

namespace LoongsonNeuq.Common;

public class StoredStudent
{
    [JsonProperty("github_id")]
    public string GitHubId { get; set; } = null!;

    [JsonProperty("research_focus")]
    public List<string> ResearchFocus { get; set; } = null!;
}