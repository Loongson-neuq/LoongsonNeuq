using LoonsonNeuq.Common;
using Newtonsoft.Json;

namespace LoonsonNeuq.ListFormatter;

public class ListRoot
{
    [JsonProperty("students")]
    public List<StoredStudent> Students { get; set; } = null!;
}