using GitHub;
using GitHub.Repos.Item.Item.Dispatches;
using LoongsonNeuq.AssignmentSubmit.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions.Serialization;
using Newtonsoft.Json;

namespace LoongsonNeuq.AssignmentSubmit.Submitters;

/// <summary>
/// Submit the result and grade to the main repository
/// </summary>
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
            _logger.LogError(e, "Failed to trigger dispatch");
            return;
        }

        _logger.LogInformation("Dispatch successfully triggered");
    }

    public class ClientPayload : DispatchesPostRequestBody_client_payload
    {
        public SubmitPayload submit_payload { get; set; } = null!;

        public override void Serialize(ISerializationWriter writer)
        {
            base.Serialize(writer);

            writer.WriteStringValue("submit_payload", JsonConvert.SerializeObject(submit_payload));
        }
    }
}