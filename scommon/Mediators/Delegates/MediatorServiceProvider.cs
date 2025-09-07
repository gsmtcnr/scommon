using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace scommon;

/// <summary>
/// Mediator service that uses delegates to build the required services
/// </summary>
public class MediatorServiceProvider : IMediatorServiceProvider
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<MediatorServiceProvider> _logger;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="provider">The service provider</param>
    /// <param name="logger">The factory logger</param>
    /// <exception cref="ArgumentNullException"></exception>
    public MediatorServiceProvider(IServiceProvider provider, ILogger<MediatorServiceProvider> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public T BuildService<T>() where T : class
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Building service for type '{type}'", typeof(T));

        var service = _provider.GetService<T>();
        return service;
    }

    /// <inheritdoc />
    public IEnumerable<T> BuildServices<T>() where T : class
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Building services for type '{type}'", typeof(T));

        return _provider.GetServices<T>();
    }
}
