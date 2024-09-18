using LoongsonNeuq.AssignmentSubmit.Configuration;
using LoongsonNeuq.AssignmentSubmit.Models;
using LoongsonNeuq.AutoGrader;
using Microsoft.Extensions.Logging;

namespace LoongsonNeuq.AssignmentSubmit;

public class GradingRunner
{
    private AssignmentConfig _assignmentConfig;
    private ILogger _logger;

    private GradingConfig GradingConfig => _assignmentConfig.AutoGrade;

    public GradingRunner(AssignmentConfig assignmentConfig, ILogger logger)
    {
        _assignmentConfig = assignmentConfig;
        _logger = logger;
    }

    public List<StepPayload?>? Run()
    {
        if (!GradingConfig.Enabled || GradingConfig.Steps is null || GradingConfig.Steps.Count == 0)
        {
            return null;
        }

        List<StepPayload?> stepPayloads = new();

        foreach (var step in GradingConfig.Steps)
        {

            var stepRunner = new StepRunner(step, _logger);

            StepResult? result = null;

            try
            {
                result = stepRunner.RunAsync().Result;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Failed to run step: {step.Title}, No score will be given. Message: {e.Message}");
            }

            if (result is not null)
            {
                stepPayloads.Add(new StepPayload
                {
                    Title = step.Title,
                    ElapsedSeconds = result.ElapsedSeconds,
                    PeakWorkingSet64 = result.PeakWorkingSet64,
                    ExitCode = result.ExitCode,
                    Score = result.Score,
                    Failed = result.Failed,
                    ReachedTimeout = result.ReachedTimeout,
                    StepResult = result,
                    StepConfig = step
                });
            }
            else
            {
                // placeholder for failed step
                stepPayloads.Add(null);
            }
        }

        return stepPayloads;
    }
}