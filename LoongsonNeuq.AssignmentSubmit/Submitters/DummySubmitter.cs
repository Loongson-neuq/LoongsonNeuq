using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LoongsonNeuq.AssignmentSubmit.Submitters;

public class DummySubmitter : ResultSubmitter
{
    private readonly ILogger _logger;

    public DummySubmitter(ILogger logger)
    {
        _logger = logger;
    }

    public override void SubmitResult()
    {
        _logger.LogInformation($"Running {nameof(DummySubmitter)}");

        _logger.LogWarning("Dummy submitter is used, no action will be taken");

        string submitPayload = JsonSerializer.Serialize(SubmitPayload, SourceGenerationContext.Default.SubmitPayload);

        _logger.LogInformation($"Submit payload:\n{submitPayload}");
    }
}
