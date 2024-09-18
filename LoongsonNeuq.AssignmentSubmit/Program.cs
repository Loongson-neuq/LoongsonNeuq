using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using LoongsonNeuq.AssignmentSubmit;
using LoongsonNeuq.Common;
using LoongsonNeuq.Common.Environments;
using LoongsonNeuq.Common.Auth;
using LoongsonNeuq.AssignmentSubmit.Submitters;
using LoongsonNeuq.AssignmentSubmit.Configuration;
using LoongsonNeuq.AssignmentSubmit.ResultSubmitters;

var services = new ServiceCollection();

services.AddLogging();

services.AddGitHubAuth()
    .WithToken<EnvTokenProvider>()
    .AddGitHubClient();

services.AddTransient<GradingRunner>();

services.AddSingleton<GitHubActions>();
services.AddSingleton<App>();

if (args.Any(arg => arg is "--debug" or "-d"))
{
    services.AddTransient<ResultSubmitter, DummySubmitter>();
}
else
{
    services.AddTransient<ResultSubmitter, BranchSubmitter>();
}

services.AddSingleton(ReadConfig);

var serviceProvider = services.BuildServiceProvider();

var result = serviceProvider.GetRequiredService<App>()
    .Run();

if (result != ExitCode.Success)
{
    Environment.Exit((int)result);
}

AssignmentConfig ReadConfig(IServiceProvider serviceProvider)
{
    var logger = serviceProvider.GetRequiredService<ILogger>();

    string? configPath = null;

    foreach (var (arg, idx) in args.Select((a, i) => (a, i)))
    {
        if (arg.StartsWith("-"))
            continue;

        if (File.Exists(arg))
        {
            configPath = arg;

            if (idx != arg.Length - 1)
            {
                logger.LogWarning($"Ignoring arguments after \"{arg}\"");
            }

            break;
        }
    }

    if (configPath is null)
    {
        if (File.Exists("config.json"))
        {
            configPath = "config.json";
        }
        else if (File.Exists(".assignment/config.json"))
        {
            configPath = ".assignment/config.json";
        }
        else
        {
            logger.LogError("No config file found, exiting");
            return null!;
        }

        logger.LogWarning($"No config file provided, using default path: {configPath}");
    }

    string json;

    try
    {
        json = File.ReadAllText(configPath);
    }
    catch (Exception e)
    {
        logger.LogError($"Failed to read config file, exiting. Message: {e.Message}");
        return null!;
    }

    return JsonSerializer.Deserialize(json, SourceGenerationContext.Default.AssignmentConfig)!;
}
