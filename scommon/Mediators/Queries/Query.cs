namespace scommon;

/// <summary>
/// Represents a data query
/// </summary>
/// <typeparam name="TResult">The result type</typeparam>
public abstract class Query<TResult> : IQuery<TResult>
{
    public IQueryCacheOptions? QueryCacheOptions { get; set; }
    public bool IgnoreTranslate { get; set; } = false;
    public bool IgnoreCache { get; set; } = false;
    public Guid TraceId { get; protected set; } = Guid.NewGuid();
}
