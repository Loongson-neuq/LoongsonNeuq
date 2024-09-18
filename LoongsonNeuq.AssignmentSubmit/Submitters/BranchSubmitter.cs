using LibGit2Sharp;
using LoongsonNeuq.AssignmentSubmit.Submitters;
using LoongsonNeuq.Common.Auth;
using LoongsonNeuq.Common.Environments;
using Microsoft.Extensions.Logging;

namespace LoongsonNeuq.AssignmentSubmit.ResultSubmitters;

public class BranchSubmitter : ResultSubmitter
{
    private readonly ILogger _logger;
    private readonly GitHubActions _gitHubActions;
    private readonly ITokenProvider _githubTokenProvider;

    private readonly string? remoteUrl;

    protected Repository repository = null!;

    public BranchSubmitter(ILogger logger, GitHubActions gitHubActions, ITokenProvider githubTokenProvider)
    {
        _logger = logger;
        _gitHubActions = gitHubActions;
        _githubTokenProvider = githubTokenProvider;

        remoteUrl = _gitHubActions.Repository is null
            ? null
            : Path.Combine("https://github.com/", _gitHubActions.Repository);
    }

    const string BranchName = "grading-result";

    protected virtual void GenerateAndStoreResults(string repoRoot)
    {

    }

    public const string DefaultRemoteName = "origin";

    protected virtual string RemoteName => DefaultRemoteName;

    public const string GitHubActionBot = "github-actions[bot]";
    public const string GitHubActionEmail = "github-actions[bot]@users.noreply.github.com";

    protected virtual Signature Author
        => new(GitHubActionBot, GitHubActionEmail, DateTime.Now);

    protected virtual Signature Committer
        => Author;

    protected virtual Commit Commit()
    {
        var sha = _gitHubActions.Sha;

        return repository.Commit($"Result for ${sha}", Author, Committer);
    }

    protected virtual PushOptions GetPushOptions()
    {
        var pushOptions = new PushOptions
        {
            CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials
            {
                Username = "x-access-token",  // GitHub uses "x-access-token" for token-based authentication
                Password = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
            }
        };

        pushOptions.OnPackBuilderProgress = (stage, current, total) =>
        {
            _logger.LogInformation($"PackBuilder: {stage} {current}/{total}");
            return true;
        };

        pushOptions.OnPushTransferProgress = (current, total, bytes) =>
        {
            _logger.LogInformation($"PushTransfer: {current}/{total} ({bytes} bytes)");
            return true;
        };

        pushOptions.OnPushStatusError = (pushStatusErrors) =>
        {
            _logger.LogError($"Failed to push the results:");
            _logger.LogError($"    Message: {pushStatusErrors.Message}.");
            _logger.LogError($"    Reference: {pushStatusErrors.Reference}.");
        };

        return pushOptions;
    }

    protected virtual void Push()
    {
        var pushOptions = GetPushOptions();

        var remoteRef = repository.Branches[RemoteName].CanonicalName;
        repository.Network.Push(repository.Network.Remotes[RemoteName], remoteRef, pushOptions);
    }

    public override void SubmitResult()
    {
        _logger.LogInformation($"Running {nameof(BranchSubmitter)}");

        if (remoteUrl is null)
        {
            _logger.LogWarning("No repository found, skipping the submission.");

            throw new InvalidOperationException("No repository found. Make sure the environment variable GITHUB_REPOSITORY is set.");
        }

        string tempRepo = Path.GetTempPath();
        _logger.LogInformation($"Using temp directory to construct the repo: {tempRepo}");

        Repository.Init(tempRepo);

        using (repository = new Repository(tempRepo))
        {
            // Create and checkout to a new branch for the grading result
            var branch = repository.CreateBranch(BranchName);
            Commands.Checkout(repository, branch);

            // Add remote repository
            repository.Network.Remotes.Add(RemoteName, remoteUrl);
            _logger.LogInformation($"Remote repository added: {remoteUrl}");

            _logger.LogInformation("Generating and storing results...");
            GenerateAndStoreResults(tempRepo);

            _logger.LogInformation("Committing the results...");
            var commit = Commit();

            _logger.LogInformation($"Committed:\n"
                + $"commit {commit.Id}\n"
                + $"Author: {commit.Author.Name} <{commit.Author.Email}>\n"
                + $"Committer: {commit.Committer.Name} <{commit.Committer.Email}>\n"
                + $"Date: {commit.Author.When}\n"
                + $"\n"
                + $"    {commit.Message}"
            );

            _logger.LogInformation("Pushing the results...");
            try
            {
                Push();
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to push the results. Message: {e.Message}");
                throw;
            }
        }

        _logger.LogInformation("Results submitted successfully.");
        _logger.LogInformation($"View the results at: {Path.Combine(remoteUrl, "tree", BranchName)}");
    }
}
