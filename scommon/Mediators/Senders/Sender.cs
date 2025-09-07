namespace scommon;

/// <summary>
/// Sends commands of a given type into the handler
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
public class Sender<TCommand> : ISender<TCommand> where TCommand : class, ICommand
{
    private readonly ICommandHandler<TCommand> _handler;
    private readonly List<IPipeline> _pipelines;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="pipelines"></param>
    public Sender(ICommandHandler<TCommand> handler, IEnumerable<IPipeline> pipelines)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _pipelines = pipelines?.ToList() ?? throw new ArgumentNullException(nameof(pipelines));
    }

    /// <inheritdoc />
    public virtual Task SendAsync(TCommand cmd, CancellationToken ct)
    {
        if (cmd == null) throw new ArgumentNullException(nameof(cmd));

        if (_pipelines == null || _pipelines.Count == 0)
            return _handler.HandleAsync(cmd, ct);

        Func<TCommand, CancellationToken, Task> next = (command, c) => _handler.HandleAsync(command, c);

        for (var i = _pipelines.Count - 1; i >= 0; i--)
        {
            var pipeline = _pipelines[i];

            var old = next;
            next = (command, c) => pipeline.OnCommandAsync(old, command, c);
        }

        return next(cmd, ct);
    }
}
