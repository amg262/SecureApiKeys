using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SecureApiKeys;

/// <summary>
/// Extension methods for setting up SecureApiKeys services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class SecureApiKeysExtensions
{
    /// <summary>
    /// Adds the SecureApiKeyGenerator to the specified <see cref="IServiceCollection"/> with default options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The service collection so that additional calls can be chained.</returns>
    public static IServiceCollection AddSecureApiKeys(this IServiceCollection services)
    {
        services.TryAddSingleton<SecureApiKeyGenerator>();
        return services;
    }

    /// <summary>
    /// Adds the SecureApiKeyGenerator to the specified <see cref="IServiceCollection"/> with custom configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">The action used to configure the options.</param>
    /// <returns>The service collection so that additional calls can be chained.</returns>
    public static IServiceCollection AddSecureApiKeys(
        this IServiceCollection services,
        Action<ApiKeyOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.TryAddSingleton<SecureApiKeyGenerator>();
        
        return services;
    }
}