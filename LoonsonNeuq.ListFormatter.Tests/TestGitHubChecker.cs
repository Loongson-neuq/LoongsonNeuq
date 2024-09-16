using LoonsonNeuq.Common;

namespace LoonsonNeuq.ListFormatter.Tests;

public class TestGitHubChecker
{
    private GitHubIDChecker _gitHubChecker;

    [SetUp]
    public void Setup()
    {
        _gitHubChecker = new GitHubIDChecker(DummyLogger.Instance);
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