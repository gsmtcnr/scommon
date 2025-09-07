using Microsoft.Extensions.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace scommon;

/// <summary>
/// Caching pipeline
/// </summary>
public class CachingPipeline : IPipeline
{
    private readonly CachingPipelineOptions _options;
    private readonly ILogger<CachingPipeline> _logger;
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CachingPipeline(
        IOptions<CachingPipelineOptions> options,
        ILogger<CachingPipeline> logger,
        ICacheManager cacheManager)
    {
        _options = options.Value;
        _logger = logger;
        _cacheManager = cacheManager;
    }

    /// <inheritdoc />
    public Task OnCommandAsync<TCommand>(Func<TCommand, CancellationToken, Task> next, TCommand cmd, CancellationToken ct)
        where TCommand : class, ICommand
    {
        return next(cmd, ct);
    }

    /// <inheritdoc />
    public async Task<TResult> OnCommandAsync<TCommand, TResult>(Func<TCommand, CancellationToken, Task<TResult>> next, TCommand cmd, CancellationToken ct)
        where TResult : IResultModel
        where TCommand : class, ICommand<TResult>
    {
        var result = await next(cmd, ct).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc />
    public Task OnEventAsync<TEvent>(Func<TEvent, CancellationToken, Task> next, TEvent evt, CancellationToken ct)
        where TEvent : class, IEvent
    {
        return next(evt, ct);
    }

    /// <inheritdoc />
    public async Task<TResult> OnQueryAsync<TQuery, TResult>(Func<TQuery, CancellationToken, Task<TResult>> next, TQuery query, CancellationToken ct)
        where TQuery : class, IQuery<TResult>
        where TResult : class, new()
    {
        if (query is { QueryCacheOptions: not null, IgnoreCache: false })
        {
            _logger.LogDebug("Starting a query from cache");
            var cachedResult = await _cacheManager.Get<TResult>(query.QueryCacheOptions.Key);
            if (cachedResult != null)
            {
                _logger.LogDebug("Returned from cache");
                return cachedResult;
            }
        }

        var result = await next(query, ct).ConfigureAwait(false);

        if (query is { QueryCacheOptions: not null, IgnoreCache: false })
        {
            var apiResponse = (IResultModel)result;
            if (apiResponse.IsSuccess)
            {
                _logger.LogDebug("Response adding to cache");
                await _cacheManager.Set(query.QueryCacheOptions.Key, result, query.QueryCacheOptions.ExpiryTime);
                _logger.LogDebug("Response added to cache");
            }
        }

        return result;
    }
}
