using System.Text;
using GitHub;
using GitHub.Models;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace LoongsonNeuq.AssignmentSubmit;

public class WebCommitChecker
{
    private readonly GitHubClient _githubClient;
    private readonly ILogger _logger;

    // B5690EEEBB952194
    private const long GITHUB_WEB_GPG_PUBLIC_KEY = -5374748261777858156;

    private static readonly string[] web_action_whitelist = new string[]
    {
        "github-actions[bot]",
        "dependabot[bot]",
        "dependabot-preview[bot]",
        "github-classroom[bot]",
    };

    private bool IsWebAction(string? pgpSignature)
    {
        if (pgpSignature is null)
            return false;

        try
        {
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(pgpSignature)))
            {
                PgpObjectFactory pgpFact = new PgpObjectFactory(PgpUtilities.GetDecoderStream(stream));

                // 读取签名
                PgpSignatureList pgpSignatureList = (PgpSignatureList)pgpFact.NextPgpObject();
                PgpSignature pgpSignatureObject = pgpSignatureList[0];

                return pgpSignatureObject.KeyId == GITHUB_WEB_GPG_PUBLIC_KEY;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to parse PGP signature");
            return false;
        }
    }

    private bool AllowedWebAction(string? actor)
    {
        return web_action_whitelist.Contains(actor)
            || actor?.EndsWith("[bot]") is true;
    }

    public WebCommitChecker(GitHubClient gitHubClient, ILogger logger)
    {
        _githubClient = gitHubClient;
        _logger = logger;
    }

    private bool UserCheck(Commit_commit commit)
    {
        if (commit.Committer?.Name is "GitHub"
            || commit.Committer?.Email is "noreply@github.com")
        {
            return commit.Author?.Name != commit.Committer.Name;
        }

        return false;
    }

    public bool CheckCommit(CommitDescriptor commitDescriptor)
    {
        if (string.IsNullOrEmpty(commitDescriptor.RepositoryName)
            || string.IsNullOrEmpty(commitDescriptor.RepositoryOwner)
            || string.IsNullOrEmpty(commitDescriptor.Sha))
        {
            _logger.LogWarning("Invalid commit descriptor");
            return false;
        }

        var payload = GetCommitInfo(commitDescriptor);

        if (payload == null)
        {
            _logger.LogWarning("Failed to get commit info");
            return false;
        }

        var commit = payload.CommitProp;
        var committer = payload.Committer?.SimpleUser?.Name;
        var author = payload?.Author?.SimpleUser?.Name;

        if (AllowedWebAction(committer) || AllowedWebAction(author))
            return true;

        if (commit is null)
        {
            _logger.LogWarning("Commit info is null");
            return false;
        }

        // Commits from git client were mostly not signed with GPG key
        if (commit?.Verification is null || commit?.Verification?.Verified is true)
            return true;

        bool detectedWebAction = IsWebAction(null!) || UserCheck(commit!);

        if (detectedWebAction)
        {
            _logger.LogError("⚠　WARNING ⚠");
            _logger.LogError("");
            _logger.LogError("  Web action detected!");
            _logger.LogError("  检测到网页端提交！");
            _logger.LogError("  ");
            _logger.LogError("  Please use git client to commit your changes.");
            _logger.LogError("  请使用 Git 客户端提交您的更改。");
            _logger.LogError("  ");
            _logger.LogError("  All your changes will be not be admitted.");
            _logger.LogError("  所有更改将不会被接受。");

            return true;
        }

        return false;
    }

    private Commit? GetCommitInfo(CommitDescriptor commit)
    {
        return _githubClient.Repos[commit.RepositoryOwner][commit.RepositoryName].Commits[commit.Sha].GetAsync()
            .ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public struct CommitDescriptor
    {
        public string RepositoryOwner { get; set; }
        public string RepositoryName { get; set; }
        public string Sha { get; set; }
    }
}
