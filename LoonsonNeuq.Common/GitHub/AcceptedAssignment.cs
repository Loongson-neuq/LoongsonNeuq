using Newtonsoft.Json;

namespace LoonsonNeuq.Common.GitHub;

public class AcceptedAssignment
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("submitted")]
    public bool Submitted { get; set; }

    [JsonProperty("passing")]
    public bool Passing { get; set; }

    [JsonProperty("commit_count")]
    public long CommitCount { get; set; }

    [JsonProperty("grade")]
    public long? Grade { get; set; }

    [JsonProperty("repository")]
    public Repository AssignmentRepository { get; set; } = null!;

    [JsonProperty("students")]
    public List<Student> Students { get; set; } = new();

    public Classroom Classroom { get; set; } = null!;
}