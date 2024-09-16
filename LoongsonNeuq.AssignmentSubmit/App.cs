using Microsoft.Extensions.Logging;
using LoongsonNeuq.AssignmentSubmit.Configuration;

namespace LoongsonNeuq.AssignmentSubmit;

public class App
{
    private readonly ILogger _logger;
    private readonly AssignmentConfig _config;

    public App(ILogger logger, AssignmentConfig config)
    {
        _logger = logger;
        _config = config;        
    }

    public ExitCode Run()
    {
        if (_config is null)
        {
            _logger.LogError("Config was not loaded, exiting");
            return ExitCode.ConfigError;
        }

        // TODO: Implement the logic here

        return ExitCode.Success;
    }
}