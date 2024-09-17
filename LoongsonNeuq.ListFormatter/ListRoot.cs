using LoongsonNeuq.Common;
using Newtonsoft.Json;

namespace LoongsonNeuq.ListFormatter;

public class ListRoot
{
    [JsonProperty("students")]
    public List<StoredStudent> Students { get; set; } = null!;
}