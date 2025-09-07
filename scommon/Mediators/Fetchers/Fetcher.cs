namespace scommon;

/// <summary>
/// Fetches data of a given query type from the handler
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public class Fetcher<TQuery, TResult> : IFetcher<TQuery, TResult> where TQuery : class, IQuery<TResult>
    where TResult : class, new()
{
    private readonly IQueryHandler<TQuery, TResult> _handler;
    private readonly List<IPipeline> _pipelines;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="pipelines"></param>
    public Fetcher(IQueryHandler<TQuery, TResult> handler, IEnumerable<IPipeline> pipelines)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _pipelines = (pipelines ?? throw new ArgumentNullException(nameof(pipelines))).ToList();
    }

    /// <inheritdoc />
    public virtual Task<TResult> FetchAsync(TQuery query, CancellationToken ct)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));

        if (_pipelines == null || _pipelines.Count == 0)
            return _handler.HandleAsync(query, ct);

        Func<TQuery, CancellationToken, Task<TResult>> next = (qry, c) => _handler.HandleAsync(qry, c);

        for (var i = _pipelines.Count - 1; i >= 0; i--)
        {
            var pipeline = _pipelines[i];

            var old = next;
            next = (qry, c) => pipeline.OnQueryAsync(old, qry, c);
        }

        return next(query, ct);
    }
}
