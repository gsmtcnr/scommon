using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace scommon;

/// <summary>
/// Entity Framework Core pipeline 
/// </summary>
public class ContextTransactionPipeline<TDbContext> : IPipeline where TDbContext : DbContext
{
    private readonly TDbContext _context;
    private readonly ICacheManager _cacheManager;
    private readonly ContextTransactionPipelineOptions _options;
    private readonly ILogger<ContextTransactionPipeline<TDbContext>> _logger;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cacheManager"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContextTransactionPipeline(
        TDbContext context,
        ICacheManager cacheManager,
        IOptions<ContextTransactionPipelineOptions> options,
        ILogger<ContextTransactionPipeline<TDbContext>> logger)
    {
        _context = context;
        _cacheManager = cacheManager;
        _options = options.Value;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task OnCommandAsync<TCommand>(Func<TCommand, CancellationToken, Task> next, TCommand cmd, CancellationToken ct) where TCommand : class, ICommand
    {
        if (!_options.BeginTransactionOnCommand)
        {
            await next(cmd, ct).ConfigureAwait(false);
            return;
        }

        _logger.LogDebug("Starting a command transaction");

        await using var tx = await _context.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        await next(cmd, ct).ConfigureAwait(false);

        _logger.LogDebug("Saving changes into the database");

        await _context.SaveChangesAsync(ct).ConfigureAwait(false);

        await tx.CommitAsync(ct);

        if (cmd.CallBroadcastFunction != null)
        {
            await cmd.CallBroadcastFunction(ct);
        }
        
        _logger.LogDebug("Changes committed into the database");
    }

    /// <inheritdoc />
    public async Task<TResult> OnCommandAsync<TCommand, TResult>(Func<TCommand, CancellationToken, Task<TResult>> next, TCommand cmd, CancellationToken ct)
        where TResult : IResultModel
        where TCommand : class, ICommand<TResult>
    {
        if (!_options.BeginTransactionOnCommand || !cmd.BeginTransactionOnCommand)
        {
            return await next(cmd, ct).ConfigureAwait(false);
        }

        _logger.LogDebug("Starting a command transaction");

        await using var tx = await _context.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        var result = await next(cmd, ct).ConfigureAwait(false);

        if (!result.IsSuccess) return result;
        
        _logger.LogDebug("Saving changes into the database");

        await _context.SaveChangesAsync(ct).ConfigureAwait(false);

        await tx.CommitAsync(ct);
        
        if (cmd.CallBroadcastFunction != null)
        {
            await cmd.CallBroadcastFunction(ct);
        }

        _logger.LogDebug("Changes committed into the database");

        return result;
    }

    /// <inheritdoc />
    public async Task OnEventAsync<TEvent>(Func<TEvent, CancellationToken, Task> next, TEvent evt, CancellationToken ct) where TEvent : class, IEvent
    {
        if (!_options.BeginTransactionOnEvent)
        {
            await next(evt, ct).ConfigureAwait(false);
            return;
        }

        _logger.LogDebug("Starting an event transaction");

        // await using var tx = await _context.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        await next(evt, ct).ConfigureAwait(false);

        _logger.LogDebug("Saving changes into the database");

        // await _context.SaveChangesAsync(ct).ConfigureAwait(false);
        //
        // await tx.CommitAsync(ct);

        _logger.LogDebug("Changes committed into the database");
    }

    /// <inheritdoc />
    public async Task<TResult> OnQueryAsync<TQuery, TResult>(Func<TQuery, CancellationToken, Task<TResult>> next, TQuery query, CancellationToken ct) where TQuery : class, IQuery<TResult>
        where TResult : class, new()
    {
        if (query is { QueryCacheOptions: not null, IgnoreCache: false })
        {
            _logger.LogDebug("Starting a query from cache");

            var cachedResult = await _cacheManager.Get<TResult>(query.QueryCacheOptions.Key);
            if (cachedResult != null)
            {
                _logger.LogDebug("Returned from memory cache");

                return cachedResult;
            }
        }

        _logger.LogDebug("Starting query");

        var result = await next(query, ct).ConfigureAwait(false);

        _logger.LogDebug("Finished query");

        //I put the data we get from the database into the cache for the next actions.
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
