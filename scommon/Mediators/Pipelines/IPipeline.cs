namespace scommon;

/// <summary>
/// Handling middleware that can be used to intercept commands, events and queries
/// </summary>
public interface IPipeline
{
    /// <summary>
    /// Method invoked when an <see cref="ICommand"/> is sent.
    /// </summary>
    /// <typeparam name="TCommand">The command type</typeparam>
    /// <param name="next">The next middleware into the chain</param>
    /// <param name="cmd">The command sent</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task to be awaited</returns>
    Task OnCommandAsync<TCommand>(Func<TCommand, CancellationToken, Task> next, TCommand cmd, CancellationToken ct)
        where TCommand : class, ICommand;

    /// <summary>
    /// Method invoked when an <see cref="ICommand{TResult}"/> is sent.
    /// </summary>
    /// <typeparam name="TCommand">The command type</typeparam>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="next">The next middleware into the chain</param>
    /// <param name="cmd">The command sent</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task to be awaited for the result</returns>
    Task<TResult> OnCommandAsync<TCommand, TResult>(Func<TCommand, CancellationToken, Task<TResult>> next, TCommand cmd, CancellationToken ct)
        where TCommand : class, ICommand<TResult>
        where TResult : IResultModel;

    /// <summary>
    /// Method invoked when an <see cref="IEvent"/> is broadcast.
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <param name="next">The next middleware into the chain</param>
    /// <param name="evt">The event broadcasted</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task to be awaited</returns>
    Task OnEventAsync<TEvent>(Func<TEvent, CancellationToken, Task> next, TEvent evt, CancellationToken ct)
        where TEvent : class, IEvent;

    /// <summary>
    /// Method invoked when an <see cref="IQuery{TResult}"/> is fetched.
    /// </summary>
    /// <typeparam name="TQuery">The query type</typeparam>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="next">The next middleware into the chain</param>
    /// <param name="query">The query to fetch</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task to be awaited for the result</returns>
    Task<TResult> OnQueryAsync<TQuery, TResult>(Func<TQuery, CancellationToken, Task<TResult>> next, TQuery query, CancellationToken ct)
        where TQuery : class, IQuery<TResult>
        where TResult : class, new();
}
