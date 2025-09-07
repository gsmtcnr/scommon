using System.Text.Json.Serialization;

namespace scommon;

/// <summary>
/// /// Represents a command
/// </summary>
public abstract class Command : ICommand
{
    /// <inheritdoc />
    public Guid TraceId { get; protected set; } = Guid.NewGuid();

    [JsonIgnore] public bool BeginTransactionOnCommand { get; set; } = true;
    [JsonIgnore]  public Func<CancellationToken, Task>? CallBroadcastFunction { get; set; }
   
}

/// <summary>
/// /// Represents a command
/// </summary>
public abstract class Command<TResult> : Command, ICommand<TResult>
{

}
