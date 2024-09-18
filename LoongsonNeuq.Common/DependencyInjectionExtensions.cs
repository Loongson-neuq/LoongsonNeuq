using System.Diagnostics.CodeAnalysis;
using GitHub;
using GitHub.Assignments;
using GitHub.Classrooms;
using GitHub.Classrooms.Item;
using GitHub.Models;
using GitHub.Octokit.Client;
using GitHub.Octokit.Client.Authentication;
using LoongsonNeuq.Common.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace LoongsonNeuq.Common;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddGitHubAuth(this IServiceCollection services) => services.AddSingleton<IAuthenticationProvider>(p =>
    {
        string token = p.GetRequiredService<ITokenProvider>().Token;

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Token is empty.");
        }

        return new TokenAuthProvider(new TokenProvider(token));
    });

    public static IServiceCollection WithToken<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceCollection services) where T : class, ITokenProvider
        => services.AddSingleton<ITokenProvider, T>();

    public static IServiceCollection AddAnonymousAuth(this IServiceCollection services)
        => services.AddSingleton<IAuthenticationProvider, AnonymousAuthenticationProvider>();

    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        services.AddSingleton<IRequestAdapter>(
                p => RequestAdapter.Create(p.GetRequiredService<IAuthenticationProvider>()));
        services.AddSingleton<GitHubClient>();

        return services;
    }

    public static IServiceCollection AddLogging<TServiceCollection>(this TServiceCollection services)
        where TServiceCollection : IServiceCollection
        // Must be concrete type to hide Microsoft's AddLogging
        => services.AddSingleton<ILogger, Logger>();
}
