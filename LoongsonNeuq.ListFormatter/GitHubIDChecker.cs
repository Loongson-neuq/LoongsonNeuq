using System.Net.Http.Headers;
using GitHub;
using LoongsonNeuq.Common;
using Microsoft.Extensions.Logging;

namespace LoongsonNeuq.ListFormatter;

public class GitHubIDChecker : IChecker
{
    private readonly ILogger _logger;

    private readonly GitHubClient _client;

    public GitHubIDChecker(ILogger logger, GitHubClient client)
    {
        _logger = logger;
        _client = client;
    }

    public bool CheckOrNormalize(ref ListRoot root)
    {
        bool allValid = true;

        foreach (var student in root.Students)
        {
            if (student.GitHubId == null)
            {
                _logger.LogError($"Missing GitHub ID found, please check and fix the list manually");

                allValid = false;
            }

            try
            {
                allValid &= CheckStudent(student).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred checking GitHub ID: {student.GitHubId}, Error: {e.Message}");

                allValid = false;
            }

        }

        return allValid;
    }

    private async Task<bool> CheckStudent(StoredStudent student)
    {
        if (!await CheckValidGitHubId(student.GitHubId))
        {
            _logger.LogError($"Invalid GitHub ID found: {student.GitHubId}, please check and fix the list");

            return false;
        }

        _logger.LogInformation($"GitHub ID is valid: {student.GitHubId}");

        return true;
    }

    /// <summary>
    /// Sending a request to GitHub to check if the ID is valid
    /// </summary>
    /// <param name="githubId">the id, not full name</param>
    /// <returns>whether valid</returns>
    private async Task<bool> CheckValidGitHubId(string githubId)
    {
        var response = await _client.Users[githubId].GetAsync();

        return response is not null && (response.PublicUser is not null || response.PrivateUser is not null);
    }
}