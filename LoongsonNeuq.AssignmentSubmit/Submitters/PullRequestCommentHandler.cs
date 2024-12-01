using System.Text.Json;
using GitHub;
using GitHub.Models;
using LoongsonNeuq.Common.Environments;
using Microsoft.Extensions.Logging;

namespace LoongsonNeuq.AssignmentSubmit.Submitters;

public class PullRequestCommentHandler
{
    private readonly ILogger _logger;
    private readonly GitHubClient _githubClient;
    private readonly GitHubActions _gitHubActions;
    private IssueComment? holderComment;

    public PullRequestCommentHandler(ILogger logger, GitHubClient gitHubClient, GitHubActions gitHubActions)
    {
        _logger = logger;
        _githubClient = gitHubClient;
        _gitHubActions = gitHubActions;
    }

    public async Task AddComment(string message)
    {
        var commentBody = new GitHub.Repos.Item.Item.Issues.Item.Comments.CommentsPostRequestBody
        {
            Body = message
        };

        var splited = _gitHubActions.Repository!.Split('/');

        string owner = _gitHubActions.RepositoryOwnerName!;
        string repo = _gitHubActions.RepositoryName!;

        string prNumber = _gitHubActions.Ref!.Split('/').Last();

        holderComment = await _githubClient.Repos[owner][repo].Issues[_gitHubActions.PrNumber!.Value]
            .Comments.PostAsync(commentBody);
    }

    public async Task UpdateComment(string message)
    {
        if (holderComment is null)
        {
            _logger.LogWarning("No comment created, cannot update comment.");
            return;
        }

        var commentBody = new GitHub.Repos.Item.Item.Issues.Comments.Item.WithComment_PatchRequestBody
        {
            Body = message
        };

        string owner = _gitHubActions.RepositoryOwnerName!;
        string repo = _gitHubActions.RepositoryName!;

        await _githubClient.Repos[owner][repo].Issues.Comments[holderComment.Id!.Value]
            .PatchAsync(commentBody);
    }
}
