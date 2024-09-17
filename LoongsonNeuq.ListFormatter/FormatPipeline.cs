using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LoongsonNeuq.ListFormatter;

public class FormatPipeline
{
    private readonly ILogger _logger;

    private readonly GitHubIdChecker _githubChecker;

    private readonly ResearchFocusNormalizer _researchChecker;

    public FormatPipeline(ILogger logger, GitHubIdChecker githubChecker, ResearchFocusNormalizer researchChecker)
    {
        _logger = logger;
        _githubChecker = githubChecker;
        _researchChecker = researchChecker;
    }

    public ExitCode Run(string[] args)
    {
        _logger.LogInformation("Starting pipeline");

        _logger.LogInformation($"Boot arguments: {string.Join(" ", args)}");

        if (args.Length == 0)
        {
            _logger.LogError("No arguments provided, exiting");

            return ExitCode.NoArguments;
        }

        if (args.Length > 1)
        {
            for (var i = 1; i < args.Length; i++)
            {
                _logger.LogWarning($"Unrecognized argument: {args[i]}");
            }
        }

        string listJsonFile = args[0];

        _logger.LogInformation($"Reading list from {listJsonFile}");

        string json;
        try
        {
            json = File.ReadAllText(listJsonFile);
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to read file: {e.Message}");

            return ExitCode.FileReadError;
        }

        _logger.LogInformation("Reading complete");
        _logger.LogInformation("Serializing list");

        // json is guaranteed to be non-null here

        var root = JsonConvert.DeserializeObject<ListRoot>(json);

        if (root is null)
        {
            _logger.LogError("Fatal error: deserialization failed, please check and fix the input file manually");
            _logger.LogError("Read JSON:");

            _logger.LogError(json);

            return ExitCode.DeserializationError;
        }

        _logger.LogInformation("Deserialization complete");

        // pass the root to the checker pipeline
        if (!_researchChecker.CheckOrNormalize(ref root))
        {
            _logger.LogError("Research focus normalization failed, exiting");

            return ExitCode.NormalizationError;
        }

        _logger.LogInformation("Research focus normalization complete");

        if (!_githubChecker.CheckOrNormalize(ref root))
        {
            _logger.LogError("GitHub ID normalization failed, exiting");

            return ExitCode.NormalizationError;
        }

        _logger.LogInformation("GitHub ID normalization complete");

        _logger.LogInformation("Normalization complete, serializing list back to JSON");

        json = JsonConvert.SerializeObject(root, Formatting.Indented);

        _logger.LogInformation("Serialization complete, saving to file");

        try
        {
            File.WriteAllText(listJsonFile, json);
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to write file: {e.Message}");

            _logger.LogError("Serialized JSON:");

            _logger.LogError(json);

            return ExitCode.FileSaveError;
        }

        _logger.LogInformation("Save complete, all operations successful");

        return ExitCode.Success;
    }
}