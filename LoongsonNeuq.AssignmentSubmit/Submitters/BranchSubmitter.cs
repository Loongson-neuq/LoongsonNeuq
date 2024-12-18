using System.Diagnostics;
using System.Text;
using System.Text.Json;
using GitHub;
using LibGit2Sharp;
using LoongsonNeuq.AssignmentSubmit.Models;
using LoongsonNeuq.AssignmentSubmit.Submitters;
using LoongsonNeuq.AutoGrader;
using LoongsonNeuq.Common.Environments;
using Microsoft.Extensions.Logging;

namespace LoongsonNeuq.AssignmentSubmit.ResultSubmitters;

public class BranchSubmitter : ResultSubmitter
{
    private readonly ILogger _logger;
    private readonly GitHubActions _gitHubActions;
    private readonly GitHubClient _githubClient;
    private readonly PullRequestCommentHandler _pullRequestCommentHandler;

    private readonly string? remoteUrl;

    private string? resultCommitSha = null;

    protected Repository repository = null!;

    public virtual string? GitHubBranchUrl => remoteUrl is not null ? Path.Combine(remoteUrl, "tree", BranchName)
        : null;

    public BranchSubmitter(ILogger logger, GitHubActions gitHubActions, GitHubClient gitHubClient, PullRequestCommentHandler pullRequestCommentHandler)
    {
        _logger = logger;
        _gitHubActions = gitHubActions;
        _githubClient = gitHubClient;
        _pullRequestCommentHandler = pullRequestCommentHandler;

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

        string readmeContent = GenerateMarkdownReport();
        File.WriteAllText(Path.Combine(repoRoot, "README.md"), readmeContent);
    }

    private static string TimestampToFormattedString(long timestamp)
    {
        const string timezoneId = "China Standard Time";
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);

        DateTimeOffset dateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(timestamp);

        DateTimeOffset dateTimeInTimeZone = TimeZoneInfo.ConvertTime(dateTimeUtc, timeZone);

        string offset = dateTimeInTimeZone.Offset.ToString(@"hh\:mm");
        string timeZoneInfo = $"UTC{(dateTimeInTimeZone.Offset >= TimeSpan.Zero ? "+" : "-")}{offset}";

        return $"{dateTimeInTimeZone.ToString("yyyy-MM-dd HH:mm:ss")} {timeZoneInfo}";
    }

    public virtual string GenerateMarkdownReport()
    {
        StringBuilder docBuilder = new();

        docBuilder.AppendLine($"# Report for {AssignmentConfig.Name}\n");
        {
            docBuilder.AppendLine("| Property | Value |");
            docBuilder.AppendLine("|:--------:|-------|");

            string sha = SubmitPayload.RepoSha;
            string url = Path.Combine(remoteUrl!, "tree", sha);

            string submitTime = TimestampToFormattedString(SubmitPayload.Timestamp);

            docBuilder.AppendLine($"| Commit | [{sha}]({url}) |");
            docBuilder.AppendLine($"| Timestamp | {submitTime} |");
            docBuilder.AppendLine($"| Assignment ID | {SubmitPayload.AssignmentId} |");

            docBuilder.AppendLine($"## Scores");
            {
                if (SubmitPayload.StepPayloads is not null)
                {
                    GenerateScoreTable(docBuilder);
                }
                else
                {
                    var configUrl = Path.Combine(remoteUrl!, "blob", _gitHubActions.Sha!, ".assignment/config.json");
                    docBuilder.AppendLine($"**Auto grading was not enabled for this assignment, see [`config.json`]({configUrl}) for more info.**\n");
                }
            }
        }

        docBuilder.AppendLine("-----------");

        docBuilder.AppendLine("*Generated by [LoongsonNeuq](https://github.com/Loongson-Neuq/LoongsonNeuq), click to view the source code.*");

        return docBuilder.ToString();
    }

    public virtual void GenerateScoreTable(StringBuilder docBuilder)
    {
        if (SubmitPayload.StepPayloads is null)
            return;

        docBuilder.AppendLine("| Step | Score | Elapsed | Peak Memory | LTE | Passed | StdOut | StdErr |");
        docBuilder.AppendLine("|:-----|:-----:|--------:|------------:|:---:|:------:|:------:|:------:|");

        foreach (var step in SubmitPayload.StepPayloads)
        {
            if (step is null)
                continue;

            var result = step.StepResult;

            docBuilder.AppendLine($"| {result.StepConfig.Title} | {Score(step)} | {ElapsedTime(result)} | {PeakMemory(result)} | {IsLTE(result.ReachedTimeout)} | {IsPassed(result.Failed)} | {Stdout(step)} | {Stderr(step)} |");
        }

        docBuilder.AppendLine();

        int totalFullScore = SubmitPayload.StepPayloads.Sum(p => p is null ? 0 : p.FullScore);
        int totalScore = SubmitPayload.StepPayloads.Sum(p => p is null ? 0 : p.Score);

        docBuilder.AppendLine($"Total score: {totalScore}/{totalFullScore}");
    }

    public virtual string Score(StepPayload step)
    {
        // 0/0 is not readable
        if (step.FullScore == 0)
            return "N/A";

        return $"{step.Score}/{step.FullScore}";
    }

    public virtual string ElapsedTime(StepResult result)
        => $"{(int)(result.ElapsedSeconds * 1000)} ms";

    private string UrlEncode(string url)
        => url.Replace(" ", "%20");

    private string urlPrefix => resultCommitSha switch
    {
        null => string.Empty,
        _ => Path.Combine(remoteUrl!, "tree", resultCommitSha)
    };

    public virtual string Stdout(StepPayload step)
    {
        if (step.StandardOutputFile is null)
            return "N/A";

        string url = Path.Combine(urlPrefix, step.OutputFolder!, step.StandardOutputFile);

        return $"[{step.StandardOutputFile}]({UrlEncode(url)})";
    }

    public virtual string Stderr(StepPayload step)
    {
        if (step.StandardErrorFile is null)
            return "N/A";

        string url = Path.Combine(urlPrefix, step.OutputFolder!, step.StandardErrorFile);

        return $"[{step.StandardErrorFile}]({UrlEncode(url)})";
    }

    public virtual string PeakMemory(StepResult? result)
    {
        if (result?.PeakWorkingSet64 is null)
            return "N/A";

        double kbytes = result.PeakWorkingSet64.Value / 1024.0;

        return $"{(int)kbytes} kb";
    }

    public virtual string IsLTE(bool lte)
        => lte ? "❗️" : "✔️";

    public virtual string IsPassed(bool failed)
        => failed ? "❌" : "✔️";

    protected virtual void StageChanges()
    {
        Commands.Stage(repository, "*");
    }

    public const string DefaultRemoteName = "origin";

    protected virtual string RemoteName => DefaultRemoteName;

    protected virtual Signature Author
        => new(GitHelper.GitHubActionBot, GitHelper.GitHubActionEmail, DateTime.Now);

    protected virtual Signature Committer
        => Author;

    protected virtual Commit Commit()
    {
        GitHelper.SetupGitConfig(_logger);

        var sha = _gitHubActions.Sha;

        // AoT deployed libgit2sharp fails include commit message, so we use git command instead
        GitHelper.RunGitCommand(_logger, $"commit -m \"Grading result for {sha}\"");

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

        GitHelper.RunGitCommand(_logger, args);
    }

    protected virtual void CheckoutToNewBranch()
    {
        GitHelper.RunGitCommand(_logger, $"checkout --orphan {BranchName}");
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

        string repoPath = GitHelper.CurrentRepo!;
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
            GitHelper.CurrentRepo = repository.Info.WorkingDirectory;
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

            resultCommitSha = commit.Id.Sha;

            // This is idiot, we can't change after the commit 
            // SubmitPayload.InfoBranch = BranchName;
            // SubmitPayload.InfoCommit = commit.Id.Sha;

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
            finally
            {
                string sha = commit.Id.Sha;
                string branchUrl = Path.Combine(remoteUrl, "tree", BranchName);
                string commitUrl = Path.Combine(remoteUrl, "tree", sha);

                string shaShort = sha[..7];

                _logger.LogInformation("Results submitted successfully.");
                _logger.LogInformation($"View the results at: {branchUrl}");
                _logger.LogInformation($"View the commit specific result at: {commitUrl}");

                CreateComment();

                void CreateComment()
                {
                    var builder = new StringBuilder();

                    builder.AppendLine($"Grading result for this commit is available at [{BranchName}({shaShort})]({commitUrl})");

                    if (SubmitPayload.StepPayloads is not null)
                    {
                        builder.AppendLine();
                        builder.AppendLine("## Score");
                        builder.AppendLine();

                        GenerateScoreTable(builder);

                        builder.AppendLine("-----------");
                        builder.AppendLine("*Generated by [LoongsonNeuq](https://github.com/Loongson-Neuq/LoongsonNeuq), click to view the source code.*");
                    }

                    CommentOnCommit(builder.ToString());

                    if (_gitHubActions.IsPullRequest)
                    {
                        _pullRequestCommentHandler.UpdateComment(builder.ToString()).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                }
            }
        }
    }

    public void CommentOnCommit(string message)
    {
        var commentBody = new GitHub.Repos.Item.Item.Commits.Item.Comments.CommentsPostRequestBody
        {
            Body = message
        };

        var splited = _gitHubActions.Repository!.Split('/');

        string owner = splited[0];
        string repo = splited[1];

        string sha = _gitHubActions.Sha!;

        _githubClient.Repos[owner][repo].Commits[sha].Comments.PostAsync(commentBody).Wait();
    }
}
