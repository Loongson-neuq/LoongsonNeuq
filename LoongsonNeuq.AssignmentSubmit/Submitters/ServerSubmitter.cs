namespace LoongsonNeuq.AssignmentSubmit.Submitters;

/// <summary>
/// Submit the result and grade to the school server
/// </summary>
public abstract class ServerSubmitter : ResultSubmitter
{
    // 提交作业的终结点
    protected abstract string SubmitEndpoint { get; }

    // 提交作业的令牌，用于验证是否确实由 GitHub Actions 发送
    // 从 GitHub Actions Secrets 中获取提交令牌以避免泄漏
    protected virtual string SubmitToken
        => throw new NotImplementedException();

    public override void SubmitResult()
    {
        throw new NotImplementedException();
    }
}
