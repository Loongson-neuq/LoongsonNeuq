using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Octokit;
using LoonsonNeuq.Common.GitHub;

namespace LoonsonNeuq.Classroom;

public class StudentsTable
{
    private readonly GitHubApi _github;

    private readonly ConcurrentDictionary<string, Student> _students = new();

    private readonly IServiceProvider _serviceProvider;

    public StudentsTable(GitHubApi github, IServiceProvider serviceProvider)
    {
        _github = github;
        _serviceProvider = serviceProvider;
    }

    public void PopulateUpdateStudents()
    {
        var response = _github.GetAuthedAsync("https://raw.githubusercontent.com/Loongson-neuq/index/master/student_list.json").Result;
        var students = JsonConvert.DeserializeObject<List<StoredStudent>>(response.Content.ReadAsStringAsync().Result)
            ?? throw new InvalidOperationException("Failed to get students.");

        var mappedStudents = students.Select(s => new Student
        {
            Login = s.GitHubLogin,
            ResearchFocus = s.ResearchFocus
        }).ToList();

        foreach (var student in mappedStudents)
        {
            if (_students.ContainsKey(student.Login))
            {
                _students.TryUpdate(student.Login, student, student);
            }
            else
            {
                _students.TryAdd(student.Login, student);
            }
        }

        var github = _serviceProvider.GetRequiredService<GitHubClient>();

        foreach (var student in _students.Values)
        {
            student.FillFields(github);
        }
    }

    private class StoredStudent
    {
        [JsonProperty("github_id")]
        public string GitHubLogin { get; set; } = null!;

        [JsonProperty("research_focus")]
        public List<string> ResearchFocus { get; set; } = null!;
    }
}