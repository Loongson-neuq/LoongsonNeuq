using LoonsonNeuq.AssignmentSubmit.Configuration;
using Microsoft.Extensions.Logging;

namespace LoonsonNeuq.AssignmentSubmit;

public class AssignmentRunner
{
    private readonly ILogger _logger;
    public AssignmentConfig Config { get; }

    public bool Enabled => Config.AutoGrade.Enabled;

    public AssignmentRunner(ILogger logger, AssignmentConfig config)
    {
        _logger = logger;
        Config = config;
    }
}