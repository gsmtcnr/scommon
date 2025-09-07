namespace scommon;

/// <summary>
/// Source : https://github.com/simplesoft-pt/Mediator
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a command to an <see cref="ICommandHandler{TCommand}"/>.
    /// </summary>
    /// <param name="cmd">The command to publish</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task to be awaited</returns>
    Task SendAsync(ICommand cmd, CancellationToken ct);

    /// <summary>
    /// Sends a command to an <see cref="ICommandHandler{TCommand,TResult}"/> and 
    /// returns the operation result.
    /// </summary>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="cmd">The command to publish</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task to be awaited for the result</returns>
    Task<TResult> SendAsync<TResult>(ICommand<TResult> cmd, CancellationToken ct);

    /// <summary>
    /// Broadcast the event across all <see cref="IEventHandler{TEvent}"/>.
    /// </summary>
    /// <param name="evt">The event to broadcast</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task to be awaited</returns>
    Task BroadcastAsync(IEvent evt, CancellationToken ct);

    /// <summary>
    /// Fetches a query from a <see cref="IQueryHandler{TQuery,TResult}"/> and 
    /// returns the query result.
    /// </summary>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="query">The query to fetch</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task to be awaited for the result</returns>
    Task<TResult> FetchAsync<TResult>(IQuery<TResult> query, CancellationToken ct);
}