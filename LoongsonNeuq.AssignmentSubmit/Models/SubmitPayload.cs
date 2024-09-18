using System.Text.Json.Serialization;

namespace LoongsonNeuq.AssignmentSubmit.Models;

public class SubmitPayload
{
    [JsonPropertyName("student")]
    public string GitHubId { get; set; } = null!;

    [JsonPropertyName("assignment_id")]
    public string AssignmentId { get; set; } = null!;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("assignment_repo")]
    public string AssignmentRepo { get; set; } = null!;

    [JsonPropertyName("repo_sha")]
    public string RepoSha { get; set; } = null!;

    [JsonPropertyName("has_info")]
    public bool HasInfo = false;

    [JsonPropertyName("info_branch")]
    public string? InfoBranch { get; set; } = null;

    [JsonPropertyName("info_commit")]
    public string? InfoCommit { get; set; } = null;

    [JsonPropertyName("steps")]
    public List<StepPayload?>? StepPayloads { get; set; } = null;
}