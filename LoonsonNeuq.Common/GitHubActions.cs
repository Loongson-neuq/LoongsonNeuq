namespace LoonsonNeuq.Common;

/// <summary>
/// Represents the GitHub Actions environment variables.
/// See https://docs.github.com/zh/actions/writing-workflows/choosing-what-your-workflow-does/store-information-in-variables#default-environment-variables
/// </summary>
public class GitHubActions
{
    public bool IsCI
        => Environment.GetEnvironmentVariable("CI") is "true";

    public string? GitHubAction
        => Environment.GetEnvironmentVariable("GITHUB_ACTION");

    public string? Actor
        => Environment.GetEnvironmentVariable("GITHUB_ACTOR");

    public string? ActorId
        => Environment.GetEnvironmentVariable("GITHUB_ACTOR_ID");

    public string? ActionPath
        => Environment.GetEnvironmentVariable("GITHUB_ACTION_PATH");    

    public string? ActionRepository
        => Environment.GetEnvironmentVariable("GITHUB_ACTION_REPOSITORY");

    public string? BaseRef
        => Environment.GetEnvironmentVariable("GITHUB_BASE_REF");

    public string? EventName
        => Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME");

    public string? StepPath
        =>  Environment.GetEnvironmentVariable("GITHUB_ENV");

    public string? HeadRef
        => Environment.GetEnvironmentVariable("GITHUB_HEAD_REF");

    public string? JobId
        => Environment.GetEnvironmentVariable("GITHUB_JOB");

    public string? Ref
        => Environment.GetEnvironmentVariable("GITHUB_REF");

    public string? RefName
        => Environment.GetEnvironmentVariable("GITHUB_REF_NAME");

    public bool? RefProtected
        => Environment.GetEnvironmentVariable("GITHUB_REF_PROTECTED") switch
        {
            "true" => true,
            "false" => false,
            _ => null
        };

    public string? RefType
        => Environment.GetEnvironmentVariable("GITHUB_REF_TYPE");

    public string? Repository
        => Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");

    public string? RepositoryId
        => Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_ID");

    public string? RepositoryOwner
        => Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER");

    public string? RepositoryOwnerId
        => Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER_ID");

    public int? RunAttempt
        => Environment.GetEnvironmentVariable("GITHUB_RUN_ATTEMPT") is { } value
            ? int.Parse(value)
            : null;

    public string? RunId
        => Environment.GetEnvironmentVariable("GITHUB_RUN_ID") is { } value
            ? value
            : null;

    public int? RunNumber
        => Environment.GetEnvironmentVariable("GITHUB_RUN_NUMBER") is { } value
            ? int.Parse(value)
            : null;

    public string? TriggerActor
        => Environment.GetEnvironmentVariable("GITHUB_TRIGGER_ACTOR");

    public string? WorkflowName
        => Environment.GetEnvironmentVariable("GITHUB_WORKFLOW");

    public string? WorkflowRef
        => Environment.GetEnvironmentVariable("GITHUB_WORKFLOW_REF");
}