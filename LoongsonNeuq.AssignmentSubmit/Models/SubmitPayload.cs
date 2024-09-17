using Newtonsoft.Json;

namespace LoongsonNeuq.AssignmentSubmit.Models;

public class SubmitPayload
{
    [JsonProperty("student")]
    public string GitHubId { get; set; } = null!;

    [JsonProperty("assignment_id")]
    public string AssignmentId { get; set; } = null!;

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }

    [JsonProperty("assignment_repo")]
    public string AssignmentRepo { get; set; } = null!;

    [JsonProperty("repo_sha")]
    public string RepoSha { get; set; } = null!;

    [JsonProperty("has_info")]
    public bool HasInfo = false;

    [JsonProperty("info_branch")]
    public string? InfoBranch { get; set; } = null;

    [JsonProperty("info_commit")]
    public string? InfoCommit { get; set; } = null;

    [JsonProperty("steps")]
    public List<StepPayload?>? StepResults { get; set; } = null;
}