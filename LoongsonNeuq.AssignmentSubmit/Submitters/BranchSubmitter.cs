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
            : Path.Combine($"https://github.com/", _gitHubActions.Repository);
    }

    const string BranchName = "grading-result";

    protected virtual void GenerateAndStoreResults(string repoRoot)
    {
        if (SubmitPayload.StepPayloads is not null)
        {
            foreach (var step in SubmitPayload.StepPayloads)
            {
                if (step is null)
                    continue;

                if (step.StepResult.StandardOutput is null
                    && step.StepResult.StandardError is null)
                {
                    continue;
                }

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

                if (step.StepResult.StandardOutput is not null)
                {
                    File.WriteAllText(Path.Combine(outputFolder, stdoutFile), step.StepResult.StandardOutput);
                    step.StandardOutputFile = stdoutFile;
                }

                if (step.StepResult.StandardError is not null)
                {
                    File.WriteAllText(Path.Combine(outputFolder, stderrFile), step.StepResult.StandardError);
                    step.StandardErrorFile = stderrFile;
                }

                step.OutputFolder = title;
            }
        }

        string serializedPayload = JsonSerializer.Serialize(SubmitPayload, SourceGenerationContext.Default.SubmitPayload);
        _logger.LogInformation($"Submit payload:\n{serializedPayload}");

        File.WriteAllText(Path.Combine(repoRoot, "result.json"), serializedPayload);
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

    private void SetupGitConfig()
    {
        repository.Config.Set("user.name", GitHubActionBot);
        repository.Config.Set("user.email", GitHubActionEmail);
    }

    protected void RunGitCommand(string gitFile, string args, string? WorkingDirectory = null)
    {
        _logger.LogInformation($"Running git command: {gitFile} {args}");

        Process git = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = gitFile,
                Arguments = args,
                UseShellExecute = true,
                CreateNoWindow = true,
                WorkingDirectory = WorkingDirectory ?? repository.Info.WorkingDirectory
            }
        };

        git.Start();
        git.WaitForExit();

        if (git.ExitCode != 0)
        {
            throw new Exception($"Failed to run git command: {git} {args}");
        }
    }

    protected virtual Commit Commit()
    {
        var sha = _gitHubActions.Sha;

        // AoT deployed libgit2sharp fails include commit message, so we use git command instead
        RunGitCommand("git", $"commit -m \"Grading result for {sha}\"");

        var commit = repository.Branches[BranchName].Commits.MaxBy(c => c.Committer.When);

        if (commit is null)
        {
            throw new Exception("No commit found.");
        }

        return commit;
    }

    protected virtual string RemoteRef => $"refs/heads/{BranchName}";

    protected virtual void Push()
    {
        // libgit2sharp does not support force push, so we use git command instead

        string args = $"push {RemoteName} {RemoteRef} --force";

        RunGitCommand("git", args);
    }

    public virtual string GetRepoPath()
        => _gitHubActions.Workspace ?? Directory.GetCurrentDirectory();

    protected virtual void CheckoutToNewBranch()
    {
        RunGitCommand("git", $"checkout --orphan {BranchName}");
    }

    public virtual void SetupRepo()
    {
        // Create and checkout to a new branch for the grading result
        CheckoutToNewBranch();

        const string gitBak = "../git-bak";
        const string gitDir = ".git";

        Directory.Move(gitDir, gitBak);
        {
            string repo = repository.Info.WorkingDirectory;

            foreach (string file in Directory.GetFiles(repo))
            {
                File.Delete(file);
            }

            foreach (string dir in Directory.GetDirectories(repo))
            {
                Directory.Delete(dir, true);
            }
        }
        Directory.Move(gitBak, gitDir);

        _logger.LogInformation($"Repository cleaned. Files: {string.Join(", ", Directory.GetFiles(repository.Info.WorkingDirectory))}");
    }

    public override void SubmitResult()
    {
        _logger.LogInformation($"Running {nameof(BranchSubmitter)}");

        if (remoteUrl is null)
        {
            _logger.LogWarning("No repository found, skipping the submission.");

            throw new InvalidOperationException("No repository found. Make sure the environment variable GITHUB_REPOSITORY is set.");
        }

        string repoPath = GetRepoPath();
        _logger.LogInformation($"Using directory to construct the repo: {repoPath}");

        if (!Directory.Exists(repoPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {repoPath}");
        }

        if (!Repository.IsValid(repoPath))
        {
            Repository.Init(repoPath);
        }

        using (repository = new Repository(repoPath))
        {
            SetupRepo();

            if (repository.Network.Remotes[RemoteName] is null)
            {
                repository.Network.Remotes.Add(RemoteName, remoteUrl);
            }

            _logger.LogInformation($"Remote repository: {repository.Network.Remotes[RemoteName].Url}");

            _logger.LogInformation("Generating and storing results...");
            GenerateAndStoreResults(repoPath);

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
