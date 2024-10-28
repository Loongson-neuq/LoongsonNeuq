using System.Text;
using LoongsonNeuq.Common;
using LoongsonNeuq.Common.Auth;
using LoongsonNeuq.RustlingsRank;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

const string MarkdownTableStartTag = "<!-- Rustlings table START -->";
const string MarkdownTableEndTag = "<!-- Rustlings table END -->";

var services = new ServiceCollection();

services.AddGitHubClient();

services.AddGitHubAuth()
    .WithToken<EnvTokenProvider>();

services.AddLogging();

services.AddSingleton<RustlingsAssignment>();
services.AddSingleton<RustingsAssignmentId>();

var provider = services.BuildServiceProvider();

var assignment = provider.GetRequiredService<RustlingsAssignment>();

await assignment.FetchAssignment();

await assignment.PopulateFinishedAssignments();

var finisheds = assignment.FinishedAssignments;

var copiedList = finisheds.ToList();

copiedList.Sort(AssignmentComparer.CompareByScore);

foreach (var item in copiedList)
{
    var score = item.DisplayScore;
    var student = item.Assignment.Students?.First().Login;

    Console.WriteLine($"{student} - {score}");
}

var markdownTable = BuildMarkdownTable(finisheds).ToString();

var readme = File.ReadAllText("README.md");

File.WriteAllText("README.md", UpdateTable(readme, markdownTable));

StringBuilder BuildMarkdownTable(IReadOnlyList<FinishedAssignment> finisheds)
{
    var sorted = finisheds.ToList();
    sorted.Sort(AssignmentComparer.CompareByScore);

    var builder = new StringBuilder();

    builder.AppendLine("| Avatar | Student | Score | Repo |");
    builder.AppendLine("|:------:|:--------|:-----:|:-----|");

    foreach (var item in sorted)
    {
        var userUrl = item.Assignment.Students?.First().HtmlUrl;
        var avatarUrl = item.Assignment.Students?.First().AvatarUrl;
        var userLogin = item.Assignment.Students?.First().Login;

        var score = item.DisplayScore;

        var repoShort = item.Assignment.Repository?.FullName;
        var repoUrl = item.Assignment.Repository?.HtmlUrl;

        var avatarHtmlNode = $"<a href=\"{userUrl}\"><img src=\"{avatarUrl}\" alt=\"{userLogin}\" width=\"48px\" height=\"48px\" /></a>";

        builder.AppendLine($"| {avatarHtmlNode} | [{userLogin}]({userUrl}) | {score} | [{repoShort}]({repoUrl}) |");
    }

    builder.AppendLine();

    builder.AppendLine($"*Last updated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}*");

    builder.AppendLine();

    return builder;
}

string UpdateTable(string oldReadme, string newTable)
{
    var oldlines = oldReadme.Replace("\r", "").Split('\n');

    var startTagLineIndex = Array.FindIndex(oldlines, x => x.Contains(MarkdownTableStartTag));
    var endTagLineIndex = Array.FindIndex(oldlines, x => x.Contains(MarkdownTableEndTag));

    if (startTagLineIndex == -1 || endTagLineIndex == -1)
    {
        provider.GetRequiredService<ILogger>().LogError("Cannot find the markdown table in README.md");
        return oldReadme;
    }

    var newLines = new List<string>();

    newLines.AddRange(oldlines.Take(startTagLineIndex + 1));
    newLines.Add(newTable);
    newLines.AddRange(oldlines.Skip(endTagLineIndex));

    return string.Join("\n", newLines);
}

public static class AssignmentComparer
{
    public static int CompareByScore(FinishedAssignment a, FinishedAssignment b)
    {
        var scoreDiff = a.DisplayScore.CompareTo(b.DisplayScore);
        if (scoreDiff != 0)
            return -scoreDiff;

        var aLogin = a.Assignment.Students?.First().Login;
        var bLogin = b.Assignment.Students?.First().Login;

        return string.Compare(aLogin, bLogin, StringComparison.Ordinal);
    }
}
