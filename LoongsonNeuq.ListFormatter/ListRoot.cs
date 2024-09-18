using System.Text.Json.Serialization;
using LoongsonNeuq.Common.Models;

namespace LoongsonNeuq.ListFormatter;

public class ListRoot
{
    [JsonPropertyName("students")]
    public List<StoredStudent> Students { get; set; } = null!;
}