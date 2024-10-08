using LoongsonNeuq.Common;

namespace LoongsonNeuq.AutoGrader.Tests;

public class TestAutoGrader
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestAutoGrader_FullScoreWhenExitProperly()
    {
        var step = new GradingStep
        {
            Title = "Test",
            Timeout = 1,
            Command = "exit 0",
            Score = 100
        };

        var runner = new StepRunner(step);

        var result = runner.RunAsync().Result;

        Assert.That(result.Score, Is.EqualTo(100));
    }

    [Test]
    public void TestAutoGrader_NoScoreWhenExitAbnormally()
    {
        var step = new GradingStep
        {
            Title = "Test",
            Timeout = 1,
            Command = "exit 1",
            Score = 100
        };

        var runner = new StepRunner(step);

        var result = runner.RunAsync().Result;

        Assert.That(result.Score, Is.EqualTo(0));
    }

    [Test]
    public void TestAutoGrader_NoScoreWhenReachedTimeLimit()
    {
        var step = new GradingStep
        {
            Title = "Test",
            Timeout = 1,
            Command = "sleep 5",
            Score = 100
        };

        var runner = new StepRunner(step);

        var result = runner.RunAsync().Result;

        Assert.That(result.Score, Is.EqualTo(0));
    }

    [Test]
    public void TestAutoGrader_TestStandaradOutputCapture()
    {
        var step = new GradingStep
        {
            Title = "Test",
            Timeout = 1,
            Command = "echo Hello",
            Score = 100
        };

        var runner = new StepRunner(step);

        var result = runner.RunAsync().Result;

        // 捕获输出时使用 AppendLine，所以输出末尾有换行符
        Assert.That(result.StandardOutput, Is.EqualTo("Hello\n"));
    }
}