using LoongsonNeuq.Common;
using LoongsonNeuq.Common.Models;

namespace LoongsonNeuq.ListFormatter.Tests;

public class TestResearchFocusNormalizer
{
    private ResearchFocusNormalizer _normalizer;

    [SetUp]
    public void Setup()
    {
        _normalizer = new ResearchFocusNormalizer(DummyLogger.Instance);
    }

    [Test]
    public void TestResearchFocus_RemoveInvalid()
    {
        var root = new ListRoot
        {
            Students = new List<StoredStudent>
            {
                new StoredStudent
                {
                    GitHubId = "octocat",
                    ResearchFocus = new List<string>
                    {
                        "CPU",
                        "OS",
                        "INVALID"
                    }
                }
            }
        };

        Assert.That(_normalizer.CheckOrNormalize(ref root), Is.True);
        Assert.That(root.Students[0].ResearchFocus, Is.EquivalentTo(new List<string> { "CPU", "OS" }));
    }

    [Test]
    public void TestResearchFocus_RemoveDuplicate()
    {
        var root = new ListRoot
        {
            Students = new List<StoredStudent>
            {
                new StoredStudent
                {
                    GitHubId = "octocat",
                    ResearchFocus = new List<string>
                    {
                        "CPU",
                        "CPU",
                        "OS",
                        "OS"
                    }
                }
            }
        };

        Assert.That(_normalizer.CheckOrNormalize(ref root), Is.True);
        Assert.That(root.Students[0].ResearchFocus, Is.EquivalentTo(new List<string> { "CPU", "OS" }));
    }

    [Test]
    public void TestResearchFocus_AllToUpperCase()
    {
        var root = new ListRoot
        {
            Students = new List<StoredStudent>
            {
                new StoredStudent
                {
                    GitHubId = "octocat",
                    ResearchFocus = new List<string>
                    {
                        "cpu",
                        "os"
                    }
                }
            }
        };

        Assert.That(_normalizer.CheckOrNormalize(ref root), Is.True);
        Assert.That(root.Students[0].ResearchFocus, Is.EquivalentTo(new List<string> { "CPU", "OS" }));
    }

    [Test]
    public void TestResearchFocus_EmptyAfterNormalization()
    {
        var root = new ListRoot
        {
            Students = new List<StoredStudent>
            {
                new StoredStudent
                {
                    GitHubId = "octocat",
                    ResearchFocus = new List<string>
                    {
                        "INVALID"
                    }
                }
            }
        };

        Assert.That(_normalizer.CheckOrNormalize(ref root), Is.False);
        Assert.That(root.Students[0].ResearchFocus, Is.Empty);
    }

    [Test]
    public void TestResearchFocus_ReportNull()
    {
        var root = new ListRoot
        {
            Students = new List<StoredStudent>
            {
                new StoredStudent
                {
                    GitHubId = "octocat",
                    ResearchFocus = null!
                }
            }
        };

        Assert.That(_normalizer.CheckOrNormalize(ref root), Is.False);
    }

    [Test]
    public void TestResearchFocus_ReportEmpty()
    {
        var root = new ListRoot
        {
            Students = new List<StoredStudent>
            {
                new StoredStudent
                {
                    GitHubId = "octocat",
                    ResearchFocus = new List<string>()
                }
            }
        };

        Assert.That(_normalizer.CheckOrNormalize(ref root), Is.False);
    }

}
