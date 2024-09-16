using System.Net.Http.Headers;
using LoongsonNeuq.Classroom;
using LoongsonNeuq.Common;
using LoongsonNeuq.Common.GitHub;
using LoongsonNeuq.Common.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Octokit;

var services = new ServiceCollection();

services.AddSingleton(new ProductInfoHeaderValue("LoongsonClassroom", "1.0"));
services.AddSingleton<ITokenProvider, EnvTokenProvider>();

services.AddSingleton<GitHubApi>();
services.AddSingleton(provider =>
{
    var github = provider.GetRequiredService<GitHubApi>();

    var classroomJson = github.GetAuthedAsync("https://raw.githubusercontent.com/Loongson-neuq/index/master/classroom.json").Result;

    var classroom = JsonConvert.DeserializeObject<Classroom>(classroomJson.Content.ReadAsStringAsync().Result)
        ?? throw new InvalidOperationException("Failed to get classroom.");

    classroom.ServiceProvider = provider;

    return classroom;
});

services.AddSingleton<ILogger, Logger>();

services.AddSingleton<StudentsTable>();

services.AddSingleton(new GitHubClient(new Octokit.ProductHeaderValue("LoongsonClassroom")));

services.AddSingleton<StudentsTable>();

var serviceProvider = services.BuildServiceProvider();

// 目前本后端仅用于 Loongson-neuq/Summary，仅会在 GitHub Actions 中运行
// 因此只有冷启动，无需考虑缓存更新问题

// var classroom = serviceProvider.GetRequiredService<Classroom>();

// var assignments = classroom.GetAssignments();

// foreach (var assignment in assignments)
// {
//     Console.WriteLine($"{assignment.Title}");

//     var accepted = assignment.GetAcceptedAssignments();

//     foreach (var aa in accepted)
//     {
//         Console.WriteLine($"  {aa.Students.Single().Login}");
//     }
// }
