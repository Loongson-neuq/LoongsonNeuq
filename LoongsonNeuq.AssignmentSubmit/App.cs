using Microsoft.Extensions.Logging;
using LoongsonNeuq.AssignmentSubmit.Configuration;
using LoongsonNeuq.AssignmentSubmit.Models;
using LoongsonNeuq.Common.Environments;
using GitHub;
using GitHub.Repos.Item.Item.Dispatches;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Microsoft.Kiota.Abstractions.Serialization;
using LoongsonNeuq.AssignmentSubmit.Submitters;

namespace LoongsonNeuq.AssignmentSubmit;

public class App
{
    private readonly ILogger _logger;
    private readonly AssignmentConfig _config;

    private readonly GitHubActions _gitHubActions;
    private readonly GitHubClient _gitHubClient;

    private readonly GradingRunner _gradingRunner;
    private readonly ResultSubmitter _resultSubmitter;
    private SubmitPayload submitPayload = new SubmitPayload();

    public App(ILogger logger, AssignmentConfig config, GitHubActions gitHubActions, GitHubClient gitHubClient, GradingRunner gradingRunner, ResultSubmitter resultSubmitter)
    {
        _logger = logger;
        _config = config;
        _gitHubActions = gitHubActions;
        _gitHubClient = gitHubClient;
        _gradingRunner = gradingRunner;
        _resultSubmitter = resultSubmitter;
    }

    private ExitCode fillSubmitPayload()
    {
        submitPayload.GitHubId = _gitHubActions.Actor!;
        submitPayload.AssignmentId = _config.CategorySpecificAssignmentId;
        submitPayload.Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        submitPayload.AssignmentRepo = _gitHubActions.Repository!;
        submitPayload.RepoSha = _gitHubActions.Sha!;

        _logger.LogInformation("Submit payload filled");
        _logger.LogInformation($"    GitHubId       : {submitPayload.GitHubId}");
        _logger.LogInformation($"    AssignmentId   : {submitPayload.AssignmentId}");
        _logger.LogInformation($"    Timestamp      : {submitPayload.Timestamp}");
        _logger.LogInformation($"    AssignmentRepo : {submitPayload.AssignmentRepo}");
        _logger.LogInformation($"    RepoSha        : {submitPayload.RepoSha}");

        if (string.IsNullOrEmpty(submitPayload.GitHubId))
        {
            _logger.LogError("Failed to get GitHubId");
            return ExitCode.FailedToGetGitHubId;
        }

        if (string.IsNullOrEmpty(submitPayload.AssignmentRepo))
        {
            _logger.LogError("Failed to get AssignmentRepo");
            return ExitCode.FailedToGetAssignmentRepo;
        }

        if (string.IsNullOrEmpty(submitPayload.RepoSha))
        {
            _logger.LogError("Failed to get RepoSha");
            return ExitCode.FailedToGetRepoSha;
        }

        return ExitCode.Success;
    }

    public ExitCode Run()
    {
        _logger.LogInformation("AssignmentSubmit started");

        if (!_gitHubActions.IsCI)
        {
            _logger.LogError("Not running in CI, exiting");
            return ExitCode.NotInCI;
        }

        var fill = fillSubmitPayload();

        if (fill != ExitCode.Success)
        {
            _logger.LogError("Failed to fill submit payload, exiting");
            return fill;
        }

        if (_config.AutoGrade.Enabled)
        {
            _logger.LogInformation("Auto grading enabled, Starting auto grading");

            submitPayload.StepResults = _gradingRunner.Run();
        }
        else
        {
            _logger.LogInformation("Auto grading disabled, Skipping auto grading");
        }

        _resultSubmitter.AssignmentConfig = _config;
        _resultSubmitter.SubmitPayload = submitPayload;

        _resultSubmitter.SubmitResult();;

        return ExitCode.Success;
    }
}