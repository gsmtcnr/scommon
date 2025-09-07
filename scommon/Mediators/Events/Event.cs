namespace scommon;

/// <summary>
/// Represents an event
/// </summary>
public abstract class Event : IEvent
{
    /// <inheritdoc />
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTimeOffset CreatedOn { get; protected set; } = DateTimeOffset.UtcNow;
}
