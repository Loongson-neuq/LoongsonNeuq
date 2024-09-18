using System.Diagnostics;
using System.Text.Json;
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
        string resultJson = JsonSerializer.Serialize(SubmitPayload, SourceGenerationContext.Default.SubmitPayload);

        File.WriteAllText(Path.Combine(repoRoot, "result.json"), resultJson);

        SubmitPayload.StepPayloads?.ForEach(step =>
        {
            if (step is null)
                return;

            var title = step.StepResult.StepConfig.Title;
            foreach (var c in Path.GetInvalidPathChars())
            {
                title = title.Replace(c, '_');
            }
            
            if (title is null)
            {
                title = Path.GetRandomFileName();
            }

            string outputFolder = Path.Combine(repoRoot, title);
            Directory.CreateDirectory(outputFolder);

            string stdoutFile = "stdout.txt";
            string stderrFile = "stderr.txt";

            File.WriteAllText(Path.Combine(outputFolder, stdoutFile), step.StepResult.StandardOutput);
            File.WriteAllText(Path.Combine(outputFolder, stderrFile), step.StepResult.StandardError);

            step.OutputFolder = title;
            step.StandardOutputFile = stdoutFile;
            step.StandardErrorFile = stderrFile;
        });
    }

    protected virtual void StageChanges()
    {
        Commands.Stage(repository, "*");
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

        return repository.Commit($"Result for {sha}", Author, Committer);
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

    protected virtual string RemoteRef => $"refs/heads/{BranchName}";

    protected virtual void Push()
    {
        // libgit2sharp does not support force push, so we use git command instead

        // var pushOptions = GetPushOptions();

        // repository.Network.Push(repository.Network.Remotes[RemoteName], RemoteRef, pushOptions);

        Process git = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"push {RemoteName} {BranchName} --force",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        git.Start();

        git.WaitForExit();

        if (git.ExitCode != 0)
        {
            throw new Exception("Failed to push the results.");
        }
    }

    public override void SubmitResult()
    {
        _logger.LogInformation($"Running {nameof(BranchSubmitter)}");

        if (remoteUrl is null)
        {
            _logger.LogWarning("No repository found, skipping the submission.");

            throw new InvalidOperationException("No repository found. Make sure the environment variable GITHUB_REPOSITORY is set.");
        }

        string tempRepo = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _logger.LogInformation($"Using temp directory to construct the repo: {tempRepo}");

        Repository.Init(tempRepo);

        using (repository = new Repository(tempRepo))
        {
            // Add remote repository
            repository.Network.Remotes.Add(RemoteName, remoteUrl);
            _logger.LogInformation($"Remote repository added: {remoteUrl}");

            _logger.LogInformation("Generating and storing results...");
            GenerateAndStoreResults(tempRepo);

            _logger.LogInformation("Staging changes...");
            StageChanges();

            _logger.LogInformation("Committing the results...");
            var commit = Commit();

            _logger.LogDebug($"Committed:\n"
                + $"commit {commit.Id}\n"
                + $"Author: {commit.Author.Name} <{commit.Author.Email}>\n"
                + $"Committer: {commit.Committer.Name} <{commit.Committer.Email}>\n"
                + $"Date: {commit.Author.When}\n"
                + $"\n"
                + $"    {commit.Message}"
            );

            SubmitPayload.InfoBranch = BranchName;
            SubmitPayload.InfoCommit = commit.Id.Sha;

            // Create and checkout to a new branch for the grading result
            // Must create branch after there are commits
            var branch = repository.CreateBranch(BranchName);
            Commands.Checkout(repository, branch);

            _logger.LogInformation($"Checked out to branch: {BranchName}");

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
