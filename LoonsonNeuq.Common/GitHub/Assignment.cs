using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace LoonsonNeuq.Common.GitHub;

public partial class Assignment
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("public_repo")]
    public bool PublicRepo { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = null!;

    [JsonProperty("type")]
    public string AssignmentType { get; set; } = null!;

    [JsonProperty("invite_link")]
    public Uri InviteLink { get; set; } = null!;

    [JsonProperty("invitations_enabled")]
    public bool InvitationsEnabled { get; set; }

    [JsonProperty("slug")]
    public string Slug { get; set; } = null!;

    [JsonProperty("students_are_repo_admins")]
    public bool StudentsAreRepoAdmins { get; set; }

    [JsonProperty("feedback_pull_requests_enabled")]
    public bool FeedbackPullRequestsEnabled { get; set; }

    [JsonProperty("accepted")]
    public long AcceptedCount { get; set; }

    [JsonProperty("submissions")]
    public long Submissions { get; set; }

    [JsonProperty("passing")]
    public long Passing { get; set; }

    public IServiceProvider ServiceProvider => Classroom.ServiceProvider;

    public Classroom Classroom { get; set; } = null!;

    public List<AcceptedAssignment> GetAcceptedAssignments()
    {
        var github = ServiceProvider.GetRequiredService<GitHubApi>();

        var response = github.GetAuthedAsync($"https://api.github.com/assignments/{Id}/accepted_assignments").Result;

        var acceptedAssignments = JsonConvert.DeserializeObject<List<AcceptedAssignment>>(response.Content.ReadAsStringAsync().Result)
            ?? throw new InvalidOperationException("Failed to get accepted assignments.");

        acceptedAssignments.ForEach(aa => aa.Classroom = Classroom);

        return acceptedAssignments;
    }
}