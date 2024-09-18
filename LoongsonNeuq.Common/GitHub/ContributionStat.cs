using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LoongsonNeuq.Common.GitHub;

public class ContributionStat
{
    [JsonPropertyName("contributions")]
    public List<Contribution> Contributions { get; } = null!;

    [JsonPropertyName("total")]
    public TotalStat Total { get; }

    private ContributionStat(List<Contribution> contributionStats)
    {
        Contributions = contributionStats;
    }

    public static async Task<ContributionStat?> Fetch(string username)
    {
        using (var webClient = new HttpClient())
        {
            var response = await webClient.GetAsync($"https://github-contributions-api.jogruber.de/v4/{username}?y=last");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return JsonSerializer.Deserialize<ContributionStat>(response.Content.ReadAsStringAsync().Result);
        }
    }

    public struct Contribution
    {
        [JsonPropertyName("date")]
        public string Date;

        [JsonPropertyName("count")]
        public int Count;

        [JsonPropertyName("level")]
        public int Level;
    }

    public struct TotalStat
    {
        [JsonPropertyName("total")]
        public int LastYearTotal;
    }
}