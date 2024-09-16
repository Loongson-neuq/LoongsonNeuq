using LoongsonNeuq.AssignmentSubmit.Configuration;

namespace LoongsonNeuq.AssignmentSubmit.Submitters;

public abstract class ResultSubmitter
{
    public AssignmentConfig AssignmentConfig { get; } = null!;

    public abstract void SubmitResult();
}