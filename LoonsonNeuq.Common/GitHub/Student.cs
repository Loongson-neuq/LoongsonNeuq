using Newtonsoft.Json;
using Octokit;

namespace LoonsonNeuq.Common.GitHub;
public class Student
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("login")]
    public string Login { get; set; } = null!;

    [JsonProperty("name")]
    public string? FullName { get; set; } = null!;

    [JsonProperty("avatar_url")]
    public string AvatarUrl { get; set; } = null!;

    [JsonProperty("html_url")]
    public string HtmlUrl { get; set; } = null!;

    public List<string>? ResearchFocus { get; set; } = null!;

    public void FillFields(GitHubClient github)
    {
        var user = github.User.Get(Login).Result;

        Id = user.Id;
        FullName = user.Name;
        AvatarUrl = user.AvatarUrl;
        HtmlUrl = user.HtmlUrl;
    }
}