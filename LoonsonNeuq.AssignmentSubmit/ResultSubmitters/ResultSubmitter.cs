using LoonsonNeuq.AssignmentSubmit.Configuration;

namespace LoonsonNeuq.AssignmentSubmit.Submitters;

public abstract class ResultSubmitter
{
    public AssignmentConfig AssignmentConfig { get; } = null!;

    public abstract void SubmitResult();
}