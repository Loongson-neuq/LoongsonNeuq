namespace LoonsonNeuq.Common.Auth;

public class ArgTokenProvider : ITokenProvider
{
    private string? _token;

    public string Token
    {
        get
        {
            if (_token is null)
            {
                _token = readToken() ??
                    throw new InvalidOperationException("Token was not provided.");
            }

            return _token;
        }
    }

    private string? readToken()
        => Environment.GetCommandLineArgs()
            .Where(arg => arg.StartsWith("github_"))
            .FirstOrDefault();
}