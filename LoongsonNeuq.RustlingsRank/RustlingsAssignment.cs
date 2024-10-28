using System.Collections.Frozen;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using GitHub;
using GitHub.Models;
using LoongsonNeuq.AssignmentSubmit.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace LoongsonNeuq.RustlingsRank;

public class RustlingsAssignment
{
    private readonly ILogger _logger;
    private readonly RustingsAssignmentId _assignmentId;
    private readonly GitHubClient _gitHubClient;

    private readonly HttpClient _httpClient = new();

    private List<ClassroomAcceptedAssignment> acceptedAssignments = null!;

    private List<FinishedAssignment> finishAssignments = new();

    public IReadOnlyList<FinishedAssignment> FinishedAssignments => finishAssignments.AsReadOnly();

    public RustlingsAssignment(ILogger logger, RustingsAssignmentId assignmentId, GitHubClient gitHubClient)
    {
        _logger = logger;
        _assignmentId = assignmentId;
        _gitHubClient = gitHubClient;

        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "LoongsonNeuq.RustlingsRank");
        _httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
    }

    public async Task FetchAssignment()
    {
        if (_assignmentId.AssignmentId is null)
        {
            throw new Exception("No assignment ID found.");
        }

        var assignments = await _gitHubClient.Assignments[_assignmentId.AssignmentId.Value!]
            .Accepted_assignments.GetAsync();

        Debug.Assert(assignments is not null);

        acceptedAssignments = assignments;

        _logger.LogInformation($"Fetched {assignments.Count} assignments.");
        _logger.LogDebug($"Assignment repos:");

        foreach (var assignment in assignments)
        {
            _logger.LogDebug($"  - {assignment.Repository?.FullName}");
        }
    }

    public async Task PopulateFinishedAssignments()
    {
        if (acceptedAssignments is null)
        {
            return;
        }

        foreach (var assignment in acceptedAssignments)
        {
            _logger.LogInformation($"Fetching submit payload for {assignment.Repository?.FullName}");

            var finishedAssignment = await FetchFinishedAssignment(assignment);
            finishAssignments.Add(finishedAssignment);
        }
    }

    private async Task<FinishedAssignment> FetchFinishedAssignment(ClassroomAcceptedAssignment assignment)
    {
        var repoFull = assignment.Repository?.FullName;
        string rawUrl = $"https://github.com/{repoFull}/raw/refs/heads/{SubmittedBranch}/{SubmittedPayloadFile}";

        var response = await _httpClient.GetAsync(rawUrl);

        if (!response.IsSuccessStatusCode)
        {
            return new FinishedAssignment(assignment, null);
        }

        var content = await response.Content.ReadAsStringAsync();

        var deserialized = DeserializePayload(content);

        if (deserialized is not null)
        {
            _logger.LogInformation($"Successfully fetched submit payload for {repoFull}");
        }

        return new FinishedAssignment(assignment, deserialized);
    }

    private static readonly string SubmittedPayloadFile = "result.json";
    private static readonly string SubmittedBranch = "grading-result";

    // TODO: Somehow the GitHub API doesn't work for fetching the submit payload.
    // It always returns null for any content.
    // private async Task<FinishedAssignment> FetchFinishedAssignment(ClassroomAcceptedAssignment assignment)
    // {
    //     string? repoFull = assignment.Repository?.FullName;

    //     if (repoFull is null)
    //         return new FinishedAssignment(assignment, null);

    //     string owner = repoFull.Split('/').First();
    //     string repo = repoFull.Split('/').Last();

    //     var payload = await _gitHubClient.Repos[owner][repo].Contents[SubmittedPayloadFile]
    //         .GetAsync(r => r.QueryParameters.Ref = SubmittedBranch);

    //     if (payload is null || payload.ContentFile is null || payload.ContentFile.Content is null)
    //         return new FinishedAssignment(assignment, null);

    //     var content = DecodeBase64String(payload.ContentFile?.Content!);

    //     var deserialized = DeserializePayload(content);

    //     if (deserialized is not null)
    //     {
    //         _logger.LogInformation($"Successfully fetched submit payload for {repoFull}");
    //     }

    //     return new FinishedAssignment(assignment, deserialized);
    // }

    private static SubmitPayload? DeserializePayload(string content)
    {
        try
        {
            return JsonSerializer.Deserialize<SubmitPayload>(content);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string DecodeBase64String(string base64String)
    {
        byte[] data = Convert.FromBase64String(base64String);
        return Encoding.UTF8.GetString(data);
    }
}
