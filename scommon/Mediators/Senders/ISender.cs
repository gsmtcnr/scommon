namespace scommon;

/// <summary>
/// Sends commands of a given type into the handler
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
public interface ISender<in TCommand> where TCommand : class, ICommand
{
    /// <summary>
    /// Sends the command
    /// </summary>
    /// <param name="cmd">The command to send</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task to be awaited</returns>
    Task SendAsync(TCommand cmd, CancellationToken ct);
}

/// <summary>
/// Sends commands of a given type into the handler
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public interface ISender<in TCommand, TResult> where TCommand : class, ICommand<TResult>
{
    /// <summary>
    /// Sends the command
    /// </summary>
    /// <param name="cmd">The command to send</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task to be awaited</returns>
    Task<TResult> SendAsync(TCommand cmd, CancellationToken ct);
}

/// <summary>
    /// Sends commands of a given type into the handler
    /// </summary>
    /// <typeparam name="TCommand">The command type</typeparam>
    /// <typeparam name="TResult">The result type</typeparam>
    public class Sender<TCommand, TResult> : ISender<TCommand, TResult> 
    where TResult : IResultModel
    where TCommand : class, ICommand<TResult>
    {
        private readonly ICommandHandler<TCommand, TResult> _handler;
        private readonly List<IPipeline> _pipelines;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="pipelines"></param>
        public Sender(ICommandHandler<TCommand, TResult> handler, IEnumerable<IPipeline> pipelines)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _pipelines = pipelines?.ToList() ?? throw new ArgumentNullException(nameof(pipelines));
        }

        /// <inheritdoc />
        public virtual Task<TResult> SendAsync(TCommand cmd, CancellationToken ct)
        {
            if (cmd == null) throw new ArgumentNullException(nameof(cmd));

            if (_pipelines == null || _pipelines.Count == 0)
                return _handler.HandleAsync(cmd, ct);

            Func<TCommand, CancellationToken, Task<TResult>> next = (command, c) => _handler.HandleAsync(command, c);

            for (var i = _pipelines.Count - 1; i >= 0; i--)
            {
                var pipeline = _pipelines[i];

                var old = next;
                next = (command, c) => pipeline.OnCommandAsync(old, command, c);
            }

            return next(cmd, ct);
        }
    }
