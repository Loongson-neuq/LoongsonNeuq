namespace LoongsonNeuq.AssignmentSubmit;

public enum ExitCode : int
{
    Success = 0,
    ConfigError = 1,
    NotInCI = 2,
    FailedToGetGitHubId = 3,
    FailedToGetAssignmentRepo = 4,
    FailedToGetRepoSha = 5,
    RequiredStepFailed = 6,
    WebActionDenied = 7,
}