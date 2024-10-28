using System.Text.Json;
using GitHub;
using Microsoft.Extensions.Logging;

namespace LoongsonNeuq.AssignmentSubmit.Submitters;

public class PullRequestCommentHandler
{
    private readonly ILogger _logger;
    private readonly GitHubClient _githubClient;
    private readonly GitHubActions _gitHubActions;
    private int? _commentId;

    public PullRequestCommentHandler(ILogger logger, GitHubClient gitHubClient, GitHubActions gitHubActions)
    {
        _logger = logger;
        _githubClient = gitHubClient;
        _gitHubActions = gitHubActions;
    }

    public async Task AddComment(string message)
    {
        var commentBody = new GitHub.Issues.Item.Comments.CommentsPostRequestBody
        {
            Body = message
        };

        var splited = _gitHubActions.Repository!.Split('/');

        string owner = splited[0];
        string repo = splited[1];

        string prNumber = Environment.GetEnvironmentVariable("GITHUB_REF").Split('/').Last();

        var comment = await _githubClient.Issues[owner][repo][prNumber].Comments.PostAsync(commentBody).ConfigureAwait(false);
        _commentId = comment.Id;
    }

    public async Task UpdateComment(string message)
    {
        if (_commentId is null)
        {
            _logger.LogWarning("No comment ID stored, cannot update comment.");
            return;
        }

        var commentBody = new GitHub.Issues.Item.Comments.Item.CommentsPatchRequestBody
        {
            Body = message
        };

        var splited = _gitHubActions.Repository!.Split('/');

        string owner = splited[0];
        string repo = splited[1];

        string prNumber = Environment.GetEnvironmentVariable("GITHUB_REF").Split('/').Last();

        await _githubClient.Issues[owner][repo][prNumber].Comments[_commentId.Value].PatchAsync(commentBody).ConfigureAwait(false);
    }
}
