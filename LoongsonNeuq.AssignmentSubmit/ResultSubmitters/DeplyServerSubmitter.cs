namespace LoongsonNeuq.AssignmentSubmit.Submitters;

/// <summary>
/// Submit the result and grade to the school server
/// </summary>
public class DeployServerSubmitter : ServerSubmitter
{
    protected override string SubmitEndpoint
        // 学校服务器太垃圾，一上强度就崩
        // 从 GitHub Actions Secrets 中获取部署服务器地址以避免终结点泄漏
        => throw new NotImplementedException();
}