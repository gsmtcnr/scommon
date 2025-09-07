namespace scommon;

/// <summary>
/// Fetches data of a given query type from the handler
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public interface IFetcher<in TQuery, TResult> where TQuery : class, IQuery<TResult>
{
    /// <summary>
    /// Fetches the query
    /// </summary>
    /// <param name="query">The query to fetch</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task to be awaited</returns>
    Task<TResult> FetchAsync(TQuery query, CancellationToken ct);
}
