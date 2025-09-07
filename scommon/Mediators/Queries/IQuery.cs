namespace scommon;

/// <summary>
/// Represents a data query
/// </summary>
/// <typeparam name="TResult">The result type</typeparam>
// ReSharper disable once UnusedTypeParameter
public interface IQuery<out TResult>
{
    /// <summary>
    /// Optional cache options 
    /// </summary>
    IQueryCacheOptions? QueryCacheOptions { get; set; }

    /// <summary>
    /// The unique identifier
    /// </summary>
    Guid TraceId { get; }
    
    bool IgnoreTranslate { get; }
    bool IgnoreCache { get; }
}

public interface IHttpQuery<out TResult> : IQuery<TResult>
{
    string BaseAddress { get; }
    int DefaultTimeout { get; } //As second example : 30 seconds.
}