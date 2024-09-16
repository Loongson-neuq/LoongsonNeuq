namespace LoongsonNeuq.ListFormatter;

public enum ExitCode : int
{
    Success = 0,
    NoArguments = 1,
    FileReadError = 2,
    DeserializationError = 3,
    NormalizationError = 4,
    FileSaveError = 5
}