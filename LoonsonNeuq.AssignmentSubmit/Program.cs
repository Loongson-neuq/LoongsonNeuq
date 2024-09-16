using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LoonsonNeuq.AssignmentSubmit;
using LoonsonNeuq.AssignmentSubmit.Configuration;
using LoonsonNeuq.Common;

var services = new ServiceCollection();

services.AddLogging();

// This is actually a captive dependency, but it's semantically correct
// just because the parent only has one instance
services.AddTransient<AssignmentRunner>();

services.AddSingleton<GitHubActions>();
services.AddSingleton<App>();

services.AddSingleton(p =>
{
    var logger = p.GetRequiredService<ILogger<Program>>();

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
        logger.LogError(e, "Failed to read config file, exiting");
        return null!;
    }

    return JsonConvert.DeserializeObject<AssignmentConfig>(json)!;
});

var serviceProvider = services.BuildServiceProvider();

var result = serviceProvider.GetRequiredService<App>()
    .Run();

if (result != ExitCode.Success)
{
    Environment.Exit((int)result);
}
