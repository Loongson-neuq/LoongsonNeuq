using Microsoft.Extensions.Logging;

namespace LoongsonNeuq.ListFormatter;

public class ResearchFocusNormalizer : IChecker
{
    private readonly ILogger _logger;

    public ResearchFocusNormalizer(ILogger logger)
    {
        _logger = logger;
    }

    public bool CheckOrNormalize(ref ListRoot root)
    {
        bool allValid = true;

        root.Students.ForEach(s =>
        {
            if (s.ResearchFocus is null or { Count: 0 })
            {
                _logger.LogError($"Missing Research Focus found, GitHub ID: {s.GitHubId}, please check and fix the list manually");

                allValid = false;

                return;
            }

            s.ResearchFocus = s.ResearchFocus!
                // Normalize the research focus
                .Where(rf => !string.IsNullOrWhiteSpace(rf))
                // Trim all fields
                .Select(rf => rf.Trim())
                // Upper case all research focus
                .Select(rf => rf.ToUpper())
                // Remove duplicates
                .Distinct()
                // Filter out invalid research focus
                .Where(rf => rf is "CPU" or "OS")
                .ToList();

            if (s.ResearchFocus.Count == 0)
            {
                _logger.LogError($"Empty field after normalization, GitHub ID: {s.GitHubId}, please check and fix the list manually");

                allValid = false;

                return;
            }

            _logger.LogInformation($"Research Focus normalized for GitHub ID: {s.GitHubId}, Result: [{string.Join(", ", s.ResearchFocus)}]");
        });

        return allValid;
    }
}