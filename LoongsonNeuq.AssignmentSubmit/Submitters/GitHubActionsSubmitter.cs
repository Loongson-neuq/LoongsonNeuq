using System.Text.Json;
using GitHub;
using GitHub.Repos.Item.Item.Dispatches;
using LoongsonNeuq.AssignmentSubmit.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions.Serialization;

namespace LoongsonNeuq.AssignmentSubmit.Submitters;

/// <summary>
/// Submit the result and grade to the main repository
/// </summary>
[Obsolete("使用 repository_dispatch 可以泄漏密钥，不要使用")]
public class GitHubActionsSubmitter : ResultSubmitter
{
    private readonly ILogger _logger;
    private readonly GitHubClient _gitHubClient;

    public GitHubActionsSubmitter(ILogger logger, GitHubClient gitHubClient)
    {
        _logger = logger;
        _gitHubClient = gitHubClient;
    }

    public override void SubmitResult()
    {
        _logger.LogInformation($"Running {nameof(GitHubActionsSubmitter)}");

        const string DstOwner = "Loongson-neuq";
        const string DstRepo = "Summary";

        var body = new DispatchesPostRequestBody
        {
            EventType = "trigger-event",
            ClientPayload = new ClientPayload
            {
                submit_payload = SubmitPayload
            }
        };

        try
        {
            _gitHubClient.Repos[DstOwner][DstRepo].Dispatches.PostAsync(body).Wait();
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to trigger dispatch, message: {e.Message}");

            throw;
        }

        _logger.LogInformation("Dispatch successfully triggered");
    }

    public class ClientPayload : DispatchesPostRequestBody_client_payload
    {
        public SubmitPayload submit_payload { get; set; } = null!;

        public override void Serialize(ISerializationWriter writer)
        {
            base.Serialize(writer);

            writer.WriteStringValue("submit_payload", JsonSerializer.Serialize(submit_payload, SourceGenerationContext.Default.SubmitPayload));
        }
    }
}