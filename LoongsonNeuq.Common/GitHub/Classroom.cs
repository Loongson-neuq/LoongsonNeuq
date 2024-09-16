using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace LoongsonNeuq.Common.GitHub;

public class Classroom
{
    public string Name { get; private set; } = null!;

    public long Id { get; private set; }

    public bool IsArchived { get; private set; }

    public string Url { get; private set; } = null!;

    public IServiceProvider ServiceProvider { get; set; } = null!;

    private static readonly ConcurrentDictionary<long, Classroom> _classrooms = new();

    public static Classroom? TryGetClassroom(long id)
    {
        return _classrooms.TryGetValue(id, out var classroom) ? classroom : null;
    }

    public List<Assignment> GetAssignments()
    {
        var github = ServiceProvider.GetRequiredService<GitHubApi>();

        var response = github.GetAuthedAsync($"https://api.github.com/classrooms/{Id}/assignments").Result;

        var assignments = JsonConvert.DeserializeObject<List<Assignment>>(response.Content.ReadAsStringAsync().Result)
            ?? throw new InvalidOperationException("Failed to get assignments.");
        assignments.ForEach(assignment => assignment.Classroom = this);

        return assignments;
    }

    public Classroom(string name, long id, string url, bool isArchived)
    {
        Name = name;
        Id = id;
        Url = url;
        IsArchived = isArchived;

        if (!_classrooms.TryAdd(id, this))
        {
            _classrooms.TryUpdate(id, this, this);
        }
    }

    public Classroom(IServiceProvider serviceProvider, string name, long id, string url, bool isArchived)
        : this(name, id, url, isArchived)
    {
        ServiceProvider = serviceProvider;
    }
}