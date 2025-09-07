using Microsoft.Extensions.DependencyInjection;

namespace scommon;

/// <summary>
/// Mediator options for container registration
/// </summary>
public class MediatorOptions
{
    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <exception cref="ArgumentNullException"></exception>
    public MediatorOptions(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    /// The service collection
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// The mediator instance lifetime. Defaults to '<see cref="ServiceLifetime.Transient"/>'.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

    /// <summary>
    /// The mediator service provider instance lifetime. Defaults to '<see cref="ServiceLifetime.Transient"/>'.
    /// </summary>
    public ServiceLifetime ServiceProviderLifetime { get; set; } = ServiceLifetime.Transient;
}
