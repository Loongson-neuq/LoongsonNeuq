using System.Text.Json.Serialization;
using LoongsonNeuq.AssignmentSubmit.Configuration;
using LoongsonNeuq.AssignmentSubmit.Models;

namespace LoongsonNeuq.AssignmentSubmit;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AssignmentConfig))]
[JsonSerializable(typeof(SubmitPayload))]
[JsonSerializable(typeof(StepPayload))]
public partial class SourceGenerationContext : JsonSerializerContext
{

}