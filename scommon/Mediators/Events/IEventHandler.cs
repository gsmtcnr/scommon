namespace scommon;

/// <summary>
/// Represents an event handler
/// </summary>
/// <typeparam name="TEvent">The event type</typeparam>
public interface IEventHandler<in TEvent> where TEvent : class, IEvent
{
    /// <summary>
    /// Handles the given event
    /// </summary>
    /// <param name="evt">The event to handle</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task to be awaited</returns>
    Task HandleAsync(TEvent evt, CancellationToken ct);
}
