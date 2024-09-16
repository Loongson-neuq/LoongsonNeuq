namespace LoongsonNeuq.Common.Auth;

public class EnvTokenProvider : ITokenProvider
{
    public string Token { get; }

    const string TokenEnvName = "GITHUB_TOKEN";

    public EnvTokenProvider()
    {
        Token = Environment.GetEnvironmentVariable(TokenEnvName)!;

        if (string.IsNullOrEmpty(Token))
        {
            throw new InvalidOperationException("GITHUB_TOKEN is not set.");
        }
    }
}