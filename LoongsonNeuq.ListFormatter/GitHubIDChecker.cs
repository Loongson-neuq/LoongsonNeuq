using System.Net.Http.Headers;
using LoongsonNeuq.Common;
using Microsoft.Extensions.Logging;

namespace LoongsonNeuq.ListFormatter;

public class GitHubIDChecker : IChecker
{
    private readonly ILogger _logger;

    private readonly HttpClient _client;

    public GitHubIDChecker(ILogger logger)
    {
        _logger = logger;

        _client = new HttpClient()
        {
            DefaultRequestHeaders =
            {
                UserAgent =
                {
                    new ProductInfoHeaderValue("ListFormatter", "1.0")
                }
            }
        };
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
        var url = $"https://api.github.com/users/{githubId}";

        HttpResponseMessage response = await _client.GetAsync(url);

        return response.IsSuccessStatusCode;
    }
}