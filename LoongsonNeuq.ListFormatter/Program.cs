using LoongsonNeuq.Common;
using LoongsonNeuq.ListFormatter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddLogging()
    .AddAnonymousAuth()
    .AddGitHubClient()
    .AddTransient<GitHubIDChecker>()
    .AddTransient<ResearchFocusNormalizer>()
    .AddSingleton<FormatPipeline>();

var serviceProvider = services.BuildServiceProvider();

var exitCode = serviceProvider.GetRequiredService<FormatPipeline>()
    .Run(args);

Environment.Exit((int)exitCode);