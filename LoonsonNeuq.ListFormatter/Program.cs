using LoonsonNeuq.Common;
using LoonsonNeuq.ListFormatter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddScoped<ILogger, Logger>();

services.AddTransient<GitHubIDChecker>();
services.AddTransient<ResearchFocusNormalizer>();

services.AddSingleton<FormatPipeline>();

var serviceProvider = services.BuildServiceProvider();

var exitCode = serviceProvider.GetRequiredService<FormatPipeline>()
    .Run(args);

Environment.Exit((int)exitCode);