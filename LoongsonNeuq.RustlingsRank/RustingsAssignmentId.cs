namespace LoongsonNeuq.RustlingsRank;

public class RustingsAssignmentId
{
    public int? AssignmentId =>
        Environment.GetEnvironmentVariable("RUSTLINGS_ASSIGNMENT_ID") is string assignmentId
            ? int.TryParse(assignmentId, out var id) ? id : null
            : null;
}