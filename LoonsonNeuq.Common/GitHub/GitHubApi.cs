using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using LoonsonNeuq.Common.Auth;

namespace LoonsonNeuq.Common.GitHub;

public class GitHubApi
{
    private readonly ProductInfoHeaderValue _productInfoHeaderValue;
    private readonly ITokenProvider _githubTokenProvider;

    private readonly ILogger _logger;

    public GitHubApi(ProductInfoHeaderValue productHeaderValue, ITokenProvider githubTokenProvider, ILogger logger)
    {
        _productInfoHeaderValue = productHeaderValue;
        _githubTokenProvider = githubTokenProvider;
        _logger = logger;
    }

    private HttpClient AuthedWebClient
        => new HttpClient
        {
            DefaultRequestHeaders =
            {
                UserAgent = { _productInfoHeaderValue },
                Authorization = new AuthenticationHeaderValue("Bearer", _githubTokenProvider.Token),
            }
        };

    private HttpClient UnauthedWebClient
        => new HttpClient
        {
            DefaultRequestHeaders =
            {
                UserAgent = { _productInfoHeaderValue },
            }
        };

    public async Task<HttpResponseMessage> GetUnauthedAsync(string url)
    {
        _logger.LogTrace($"[GitHubApi] [GET] {url}");
        using var client = UnauthedWebClient;

        return await client.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetUnauthedRetryAsync(string url, double retrySeconds = 1, int retryCount = 10)
    {
        for (var i = 0; i < retryCount; i++)
        {
            var response = await GetUnauthedAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            _logger.LogWarning($"{i + 1} tries failed to get {url} with status code {response.StatusCode}. Retrying in {retrySeconds} seconds...");

            await Task.Delay(TimeSpan.FromSeconds(retrySeconds));
        }

        throw new HttpRequestException($"Failed to get {url} after {retryCount} retries.");
    }

    public async Task<HttpResponseMessage> GetAuthedRetryAsync(string url, double retrySeconds = 1, int retryCount = 10)
    {
        for (var i = 0; i < retryCount; i++)
        {
            var response = await GetAuthedAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            _logger.LogWarning($"{i + 1} tries failed to get {url} with status code {response.StatusCode}. Retrying in {retrySeconds} seconds...");

            await Task.Delay(TimeSpan.FromSeconds(retrySeconds));
        }

        throw new HttpRequestException($"Failed to get {url} after {retryCount} retries.");
    }

    public async Task<HttpResponseMessage> GetAuthedAsync(string url)
    {
        _logger.LogTrace($"[Authed] [GitHubApi] [GET] {url}");
        using var client = AuthedWebClient;

        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        return await client.GetAsync(url);
    }
}