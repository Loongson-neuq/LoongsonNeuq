using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using LoongsonNeuq.AssignmentSubmit;
using LoongsonNeuq.Common;
using LoongsonNeuq.Common.Environments;
using LoongsonNeuq.Common.Auth;
using LoongsonNeuq.AssignmentSubmit.Submitters;

var services = new ServiceCollection();

services.AddLogging();

services.AddGitHubAuth()
    .WithToken<EnvTokenProvider>()
    .AddGitHubClient();

services.AddTransient<GradingRunner>();

services.AddSingleton<GitHubActions>();
services.AddSingleton<App>();

services.AddTransient<ResultSubmitter, GitHubActionsSubmitter>();

services.AddSingleton(p =>
{
    var logger = p.GetRequiredService<ILogger>();

    string configPath;

    if (args.Length == 0)
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
    else
    {
        if (args.Length > 1)
            logger.LogError("Too many arguments provided, only taking the first one as the config file path");

        configPath = args[0];
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
});

var serviceProvider = services.BuildServiceProvider();

var result = serviceProvider.GetRequiredService<App>()
    .Run();

if (result != ExitCode.Success)
{
    Environment.Exit((int)result);
}
