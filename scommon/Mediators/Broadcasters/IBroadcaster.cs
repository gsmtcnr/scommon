namespace scommon;

/// <summary>
/// Broadcasts an event of a given type to all the handlers
/// </summary>
/// <typeparam name="TEvent">The event type</typeparam>
public interface IBroadcaster<in TEvent> where TEvent : class, IEvent
{
    /// <summary>
    /// Broadcasts the event
    /// </summary>
    /// <param name="evt">The event to broadcast</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task to be awaited</returns>
    Task BroadcastAsync(TEvent evt, CancellationToken ct);
}