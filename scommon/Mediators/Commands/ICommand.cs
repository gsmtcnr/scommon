namespace scommon;

/// <summary>
/// Represents a command
/// </summary>
public interface ICommand
{
    /// <summary>
    /// The unique identifier
    /// </summary>
    Guid TraceId { get; }

    bool BeginTransactionOnCommand { get; set; }
    
    Func<CancellationToken, Task>? CallBroadcastFunction { get; set; }
    
}

/// <summary>
/// Represents a command with a result
/// </summary>
/// <typeparam name="TResult">The result type</typeparam>
// ReSharper disable once UnusedTypeParameter
public interface ICommand<out TResult> : ICommand
{
}
