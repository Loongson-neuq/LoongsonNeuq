using System.Collections.Concurrent;
using LoongsonNeuq.Common;
using GitHub;
using System.Diagnostics;
using GitHub.Models;
using System.Collections.Frozen;
using LoongsonNeuq.Common.Models;
using System.Text.Json;

namespace LoongsonNeuq.Classroom;

public class StudentsTable
{
    private readonly GitHubClient _github;

    private FrozenDictionary<string, StoredStudent>? _students = null;

    private readonly IServiceProvider _serviceProvider;

    public FrozenDictionary<string, StoredStudent> Students
    {
        get
        {
            if (_students is null)
            {
                PopulateStudents();
            }

            return _students!;
        }
    }

    public StudentsTable(GitHubClient github, IServiceProvider serviceProvider)
    {
        _github = github;
        _serviceProvider = serviceProvider;
    }

    private string? ReadContent(ContentFile? contentFile)
    {
        if (contentFile is null)
        {
            return null;
        }

        Debug.Assert(contentFile.Encoding == "base64");

        return contentFile.Content;
    }

    public void PopulateStudents()
    {
        const string owner = "Loongson-neuq";
        const string repo = "index";

        const string path = "student_list.json";

        var response = _github.Repos[owner][repo].Contents[path].GetAsync().Result;

        Debug.Assert(response is not null);

        string? content = ReadContent(response.ContentFile);

        if (content is null)
        {
            throw new InvalidOperationException("Failed to get students.");
        }

        var students = JsonSerializer.Deserialize<List<StoredStudent>>(content)
            ?? throw new InvalidOperationException("Failed to deserialize students.");

        _students = students.ToDictionary(s => s.GitHubId).ToFrozenDictionary();
    }
}