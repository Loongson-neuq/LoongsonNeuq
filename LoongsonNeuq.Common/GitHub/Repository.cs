using Newtonsoft.Json;

namespace LoongsonNeuq.Common.GitHub;

public class Repository
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("full_name")]
    public string FullName { get; set; } = null!;

    [JsonProperty("html_url")]
    public string HtmlUrl { get; set; } = null!;

    [JsonProperty("private")]
    public bool Private { get; set; }

    [JsonProperty("default_branch")]
    public string DefaultBranch { get; set; } = null!;
}