namespace scommon;

/// <summary>
/// Handling middleware that can be used to intercept commands events and queries
/// </summary>
public abstract class Pipeline : IPipeline
{
    /// <inheritdoc />
    public virtual Task OnCommandAsync<TCommand>(Func<TCommand, CancellationToken, Task> next, TCommand cmd, CancellationToken ct)
        where TCommand : class, ICommand
    {
        return next(cmd, ct);
    }

    /// <inheritdoc />
    public virtual Task<TResult> OnCommandAsync<TCommand, TResult>(Func<TCommand, CancellationToken, Task<TResult>> next, TCommand cmd, CancellationToken ct)
        where TResult : IResultModel
        where TCommand : class, ICommand<TResult>
    {
        return next(cmd, ct);
    }

    /// <inheritdoc />
    public virtual Task OnEventAsync<TEvent>(Func<TEvent, CancellationToken, Task> next, TEvent evt, CancellationToken ct)
        where TEvent : class, IEvent
    {
        return next(evt, ct);
    }

    /// <inheritdoc />
    public virtual Task<TResult> OnQueryAsync<TQuery, TResult>(Func<TQuery, CancellationToken, Task<TResult>> next, TQuery query, CancellationToken ct)
        where TQuery : class, IQuery<TResult>
        where TResult : class, new()
    {
        return next(query, ct);
    }
}
