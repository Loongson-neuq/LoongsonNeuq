using GitHub.Models;
using LoongsonNeuq.AssignmentSubmit.Models;

namespace LoongsonNeuq.RustlingsRank;

public class FinishedAssignment
{
    public FinishedAssignment(ClassroomAcceptedAssignment assignment, SubmitPayload? submittedPayload)
    {
        Assignment = assignment;
        SubmittedPayload = submittedPayload;
    }

    public ClassroomAcceptedAssignment Assignment { get; private set; } = null!;

    public SubmitPayload? SubmittedPayload { get; private set; }

    public int? TotalScore => SubmittedPayload?.StepPayloads?.Sum(x => x?.Score);

    public int DisplayScore => TotalScore ?? 0;
}