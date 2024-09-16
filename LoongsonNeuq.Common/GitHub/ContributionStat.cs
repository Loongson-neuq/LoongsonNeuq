using Newtonsoft.Json;

namespace LoongsonNeuq.Common.GitHub;

public class ContributionStat
{
    [JsonProperty("contributions")]
    public List<Contribution> Contributions { get; } = null!;

    [JsonProperty("total")]
    public TotalStat Total { get; }

    private ContributionStat(List<Contribution> contributionStats)
    {
        Contributions = contributionStats;
    }

    public static async Task<ContributionStat> Fetch(GitHubApi github, string username)
    {
        var response = await github.GetUnauthedAsync($"https://github-contributions-api.jogruber.de/v4/{username}?y=last");

        var contributions = JsonConvert.DeserializeObject<ContributionStat>(response.Content.ReadAsStringAsync().Result)
            ?? throw new Exception("Failed to get contributions.");

        return contributions;
    }

    public struct Contribution
    {
        [JsonProperty("date")]
        public string Date;

        [JsonProperty("count")]
        public int Count;

        [JsonProperty("level")]
        public int Level;
    }

    public struct TotalStat
    {
        [JsonProperty("total")]
        public int LastYearTotal;
    }
}