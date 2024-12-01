using Microsoft.Extensions.Logging;
using LoongsonNeuq.AssignmentSubmit.Configuration;
using LoongsonNeuq.AssignmentSubmit.Models;
using LoongsonNeuq.Common.Environments;
using GitHub;
using GitHub.Repos.Item.Item.Dispatches;
using System.Text.Json.Serialization;
using Microsoft.Kiota.Abstractions.Serialization;
using LoongsonNeuq.AssignmentSubmit.Submitters;
using System.Text.Json;
using static LoongsonNeuq.AssignmentSubmit.WebCommitChecker;

namespace LoongsonNeuq.AssignmentSubmit;

public class App
{
    private readonly ILogger _logger;
    private readonly AssignmentConfig _config;

    private readonly GitHubActions _gitHubActions;

    private readonly GradingRunner _gradingRunner;
    private readonly ResultSubmitter _resultSubmitter;
    private readonly WebCommitChecker _webCommitChecker;
    private readonly GitHubClient _gitHubClient;
    private readonly PullRequestCommentHandler _pullRequestCommentHandler;
    private SubmitPayload submitPayload = new SubmitPayload();

    public App(ILogger logger, AssignmentConfig config, GitHubActions gitHubActions, GradingRunner gradingRunner, ResultSubmitter resultSubmitter, WebCommitChecker webCommitChecker, GitHubClient gitHubClient, PullRequestCommentHandler pullRequestCommentHandler)
    {
        _logger = logger;
        _config = config;
        _gitHubActions = gitHubActions;
        _gradingRunner = gradingRunner;
        _resultSubmitter = resultSubmitter;
        _webCommitChecker = webCommitChecker;
        _gitHubClient = gitHubClient;
        _pullRequestCommentHandler = pullRequestCommentHandler;

        BypassSubmit = new(() => ShouldBypassSubmit());
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

        if (_config == null)
        {
            return ExitCode.ConfigError;
        }

        if (BypassSubmit.Value)
        {
            _logger.LogDebug("Bypassing submit matched");
            return ExitCode.Success;
        }

        if (!_gitHubActions.IsCI)
        {
            _logger.LogError("Not running in CI, exiting");
            return ExitCode.NotInCI;
        }

        GitHelper.InitCurrentRepoWithDefault();

        // if (_webCommitChecker.CheckCommit(new CommitDescriptor(
        //     _gitHubActions.Repository!,
        //     _gitHubActions.Sha!)))
        // {
        //     _logger.LogError("Nothing will be submitted");
        //     return ExitCode.WebActionDenied;
        // }

        var fill = fillSubmitPayload();

        if (fill != ExitCode.Success)
        {
            _logger.LogError("Failed to fill submit payload, exiting");
            return fill;
        }

        // This requires you enable Pull request in the workflow file
        if (_gitHubActions.IsPullRequest)
        {
            _logger.LogInformation("CI triggered by pull request, generating placeholder comment");
            _pullRequestCommentHandler.AddComment("Grading in progress...\n\n" +
                "Please wait for the grading result. \n\n" +
                "The grading result will be updated here once it's done.")
                    .ConfigureAwait(false).GetAwaiter().GetResult();
        }

        if (_config.AutoGrade.Enabled)
        {
            _logger.LogInformation("Auto grading enabled, Starting auto grading");

            submitPayload.StepPayloads = _gradingRunner.Run();
        }
        else
        {
            _logger.LogInformation("Auto grading disabled, Skipping auto grading");
        }

        _resultSubmitter.AssignmentConfig = _config;
        _resultSubmitter.SubmitPayload = submitPayload;

        _resultSubmitter.SubmitResult();

        bool hasRequiredStepFailed = submitPayload.StepPayloads is not null
            && submitPayload.StepPayloads.Any(
                step => step?.StepConfig.Required is true && step.Failed);

        if (hasRequiredStepFailed)
        {
            _logger.LogError("Some required steps failed:");

            foreach (var step in submitPayload.StepPayloads!)
            {
                if (step?.StepConfig.Required is true && step.Failed)
                {
                    _logger.LogError($"    {step.StepConfig.Title}");
                }
            }

            _logger.LogError("See details in the result.json file");

            return ExitCode.RequiredStepFailed;
        }

        return ExitCode.Success;
    }

    private readonly Lazy<bool> BypassSubmit;

    private bool ShouldBypassSubmit()
    {
        string[] repository = _gitHubActions.Repository!.Split('/');
        string owner = repository[0];
        string repoName = repository[1];

        var repo = _gitHubClient.Repos[owner][repoName].GetAsync().GetAwaiter().GetResult();

        // Don't know what should be done here
        if (repo is null)
        {
            _logger.LogWarning("Failed to get repo info");
            return true;
        }

        if (repo.TemplateRepository is null)
            return false;

        if (repo.TemplateRepository.FullName == "Loongson-neuq/AssignmentTemplate")
            return true;

        if (repo.IsTemplate is true)
            return true;

        if (repo.Fork is false)
            return true;

        // _logger.LogWarning("Unknown repo type, bypassing web action check");
        return false;
    }
}
