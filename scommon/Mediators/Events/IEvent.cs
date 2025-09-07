namespace scommon;

/// <summary>
/// Represents an event
/// </summary>
public interface IEvent
{
    /// <summary>
    /// The unique identifier
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// The date and time in which the instance was created
    /// </summary>
    DateTimeOffset CreatedOn { get; }
    
}