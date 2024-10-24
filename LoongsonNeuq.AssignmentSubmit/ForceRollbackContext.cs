using System.Diagnostics;
using GitHub.Models;
using LoongsonNeuq.Common.Environments;
using Microsoft.Extensions.Logging;

namespace LoongsonNeuq.AssignmentSubmit;

public class ForceRollbackContext
{
    private readonly Commit? _commit;
    private readonly ILogger _logger;

    public ForceRollbackContext(ILogger logger, Commit? commit)
    {
        _logger = logger;
        _commit = commit;
    }

    private string PreviousCommitUrl => _commit?.HtmlUrl!;

    public string GenerateWarningMessage(DiffEntry diff)
    {
        return $"""
                # ⚠　WARNING ⚠
                ## Web action detected!
                ## 检测到网页端提交！

                The commit was made by a web action, which is not allowed to use in this repository. It has been forced to rollback.
                网页端被禁止使用，该提交已经被强制撤回。
                    
                ## Previous file page url:
                ## 被撤回前文件的页面链接:
                
                [{diff.Filename}@{diff.Sha?[..7]}]({diff.BlobUrl})
                
                ## Previous file raw url:
                ## 被撤回前文件的下载链接:
                
                [{diff.Filename}@{diff.Sha?[..7]}]({diff.RawUrl})
                """;
    }

    public virtual void RollbackCommit()
        => GitHelper.RunGitCommand(_logger, "reset --soft HEAD^");

    public virtual void StageAllFiles()
        => GitHelper.RunGitCommand(_logger, "add .");

    public virtual string Ref => new GitHubActions().Ref!;

    public virtual string OriginName => "origin";

    public virtual void ForcePushRollback()
        => GitHelper.RunGitCommand(_logger, $"push {OriginName} {Ref} --force");

    public virtual void CreateRevertCommit()
        => GitHelper.RunGitCommand(_logger, $"commit -m \"Revert commit {new GitHubActions().Sha} for using web UI\"");

    public void RollbackLocalFiles()
    {
        Debug.Assert(GitHelper.CurrentRepo is not null);

        _logger.LogInformation("Rolling back local files...");
        RollbackCommit();

        if (_commit is null)
            throw new InvalidOperationException("Commit payload is null");

        if (_commit.Files is null)
            return;

        List<DiffEntry> diffs = _commit.Files!;

        _logger.LogInformation("Rolling back local files...");
        foreach (var diff in diffs)
        {
            if (diff is null)
                continue;

            _logger.LogInformation($"Rolling back {diff.Filename}...");

            var fileName = diff.Filename!;
            var fullpath = Path.Combine(GitHelper.CurrentRepo, fileName);

            CreateRemovedFile(fullpath, diff);
        }

        _logger.LogInformation("Staging all files...");
        StageAllFiles();

        _logger.LogInformation("Force pushing rollback...");
        ForcePushRollback();
    }

    private void CreateRemovedFile(string fullpath, DiffEntry diff)
    {
        if (File.Exists(fullpath))
            File.Delete(fullpath);
        // WriteAllText will create the file if it doesn't exist

        string newContent = GenerateWarningMessage(diff);

        File.WriteAllText(fullpath, newContent);
    }
}
