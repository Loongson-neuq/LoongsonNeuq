using LoongsonNeuq.AssignmentSubmit.Configuration;
using LoongsonNeuq.AssignmentSubmit.Models;
using LoongsonNeuq.AutoGrader;

namespace LoongsonNeuq.AssignmentSubmit.Submitters;

public abstract class ResultSubmitter
{
    public AssignmentConfig AssignmentConfig { get; set; } = null!;

    public SubmitPayload SubmitPayload { get; set; } = null!;

    public abstract void SubmitResult();
}