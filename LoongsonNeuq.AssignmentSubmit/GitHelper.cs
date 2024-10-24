using System.Diagnostics;
using LoongsonNeuq.Common.Environments;
using Microsoft.Extensions.Logging;

namespace LoongsonNeuq.AssignmentSubmit;

public class GitHelper
{
    public static string? CurrentRepo = null;

    public static string GetDefaultRepoPath()
        => new GitHubActions().Workspace ?? Directory.GetCurrentDirectory();

    public static string InitCurrentRepoWithDefault()
    {
        CurrentRepo = GetDefaultRepoPath();

        Debug.Assert(CurrentRepo is not null, "CurrentRepo should not be null");

        return CurrentRepo;
    }
    
    public const string GitHubActionBot = "github-actions[bot]";
    public const string GitHubActionEmail = "github-actions[bot]@users.noreply.github.com";

    public static void SetupGitConfig(ILogger logger)
    {
        // there's seems issues in string marshaller in libgit2sharp
        // All strings are marshalled as empty strings, so we use git command instead
        GitHelper.RunGitCommand(logger, $"config user.name \"{GitHubActionBot}\"");
        GitHelper.RunGitCommand(logger, $"config user.email \"{GitHubActionEmail}\"");
    }


    public static void RunGitCommand(ILogger logger, string args, string? WorkingDirectory = null, string gitFile = "git")
    {
        logger.LogInformation($"Running git command: {gitFile} {args}");

        Process git = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = gitFile,
                Arguments = args,
                UseShellExecute = true,
                CreateNoWindow = true,
                WorkingDirectory = WorkingDirectory ?? CurrentRepo ?? Environment.CurrentDirectory
            }
        };

        git.Start();
        git.WaitForExit();

        if (git.ExitCode != 0)
        {
            throw new Exception($"Failed to run git command: {git} {args}");
        }
    }
}