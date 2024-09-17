using System.Diagnostics.CodeAnalysis;
using System.Net;
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

    public static async Task<ContributionStat?> Fetch(string username)
    {
        using (var webClient = new HttpClient())
        {
            var response = await webClient.GetAsync($"https://github-contributions-api.jogruber.de/v4/{username}?y=last");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<ContributionStat>(response.Content.ReadAsStringAsync().Result);
        }
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