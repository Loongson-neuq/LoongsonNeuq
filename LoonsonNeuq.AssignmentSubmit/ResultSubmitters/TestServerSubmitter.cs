namespace LoonsonNeuq.AssignmentSubmit.Submitters;

/// <summary>
/// Submit the result and grade to the school server
/// </summary>
public class TestServerSubmitter : ServerSubmitter
{
    protected override string SubmitEndpoint
        => "https://ls-assign.caiyi1.me/";
}