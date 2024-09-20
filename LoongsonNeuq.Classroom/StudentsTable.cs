using System.Collections.Concurrent;
using LoongsonNeuq.Common;
using GitHub;
using System.Diagnostics;
using GitHub.Models;
using System.Collections.Frozen;
using LoongsonNeuq.Common.Models;
using System.Text.Json;
using System.Text;

namespace LoongsonNeuq.Classroom;

public class StudentsTable
{
    private readonly GitHubClient _github;

    private readonly IServiceProvider _serviceProvider;

    public List<StoredStudent> RegisteredStudents { get; private set; } = null!;

    public StudentsTable(GitHubClient github, IServiceProvider serviceProvider)
    {
        _github = github;
        _serviceProvider = serviceProvider;
    }

    private string? ReadContent(ContentFile? contentFile)
    {
        if (contentFile is null || contentFile.Content is null)
        {
            return null;
        }

        Debug.Assert(contentFile.Encoding == "base64");

        // Decode base64
        byte[] data = Convert.FromBase64String(contentFile.Content);
        return Encoding.UTF8.GetString(data);
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

        RegisteredStudents = JsonSerializer.Deserialize<List<StoredStudent>>(content)
            ?? throw new InvalidOperationException("Failed to deserialize students.");
    }
}