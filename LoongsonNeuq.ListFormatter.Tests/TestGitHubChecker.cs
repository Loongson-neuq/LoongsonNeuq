using GitHub;
using GitHub.Octokit.Client;
using LoongsonNeuq.Common;
using LoongsonNeuq.Common.Models;
using Microsoft.Kiota.Abstractions.Authentication;

namespace LoongsonNeuq.ListFormatter.Tests;

public class TestGitHubChecker
{
    private GitHubIdChecker _gitHubChecker;

    [SetUp]
    public void Setup()
    {
        if (_gitHubChecker != null)
            return;

        var adapter = RequestAdapter.Create(new AnonymousAuthenticationProvider());
        var githubClient = new GitHubClient(adapter);

        _gitHubChecker = new GitHubIdChecker(DummyLogger.Instance, githubClient);
    }

    [Test]
    public void TestId_ReportInvalid()
    {
        var root = new ListRoot
        {
            Students = new List<StoredStudent>
            {
                new StoredStudent
                {
                    GitHubId = "DEFINITELY_NOT_A_GITHUB_ID"
                }
            }
        };

        Assert.That(_gitHubChecker.CheckOrNormalize(ref root), Is.False);
    }

    [Test]
    public void TestId_ReportMissing()
    {
        var root = new ListRoot
        {
            Students = new List<StoredStudent>
            {
                new StoredStudent
                {
                    GitHubId = null!
                }
            }
        };

        Assert.That(_gitHubChecker.CheckOrNormalize(ref root), Is.False);
    }

    public void TestId_ReportValid()
    {
        var root = new ListRoot
        {
            Students = new List<StoredStudent>
            {
                new StoredStudent
                {
                    GitHubId = "octocat"
                }
            }
        };

        Assert.That(_gitHubChecker.CheckOrNormalize(ref root), Is.True);
    }
}