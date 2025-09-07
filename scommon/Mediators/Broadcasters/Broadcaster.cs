namespace scommon;

/// <summary>
/// Broadcasts an event of a given type to all the handlers
/// </summary>
/// <typeparam name="TEvent">The event type</typeparam>
public class Broadcaster<TEvent> : IBroadcaster<TEvent> where TEvent : class, IEvent
{
    private readonly IEnumerable<IEventHandler<TEvent>> _handlers;
    private readonly List<IPipeline> _pipelines;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="handlers"></param>
    /// <param name="pipelines"></param>
    public Broadcaster(IEnumerable<IEventHandler<TEvent>> handlers, IEnumerable<IPipeline> pipelines)
    {
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        _pipelines = pipelines?.ToList() ?? throw new ArgumentNullException(nameof(pipelines));
    }

    /// <inheritdoc />
    public virtual Task BroadcastAsync(TEvent evt, CancellationToken ct)
    {
        if (evt == null) throw new ArgumentNullException(nameof(evt));

        Func<TEvent, CancellationToken, Task> next = (@event, cancellationToken) =>
        {
            if (_handlers == null)
            {
                return Task.CompletedTask;
            }

            var handlers = _handlers
                .Select(handler => handler.HandleAsync(@event, cancellationToken))
                .ToArray();

            if (handlers.Length == 0)
            {
                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource<bool>();

            Task.Factory.ContinueWhenAll(handlers, tasks =>
            {
                List<Exception> exceptions = null;
                foreach (var t in tasks)
                {
                    if (t.Exception == null)
                        continue;

                    exceptions ??= new List<Exception>(tasks.Length);
                    exceptions.Add(t.Exception.InnerException!);
                }

                if (exceptions == null)
                    tcs.SetResult(true);
                else
                    tcs.SetException(new AggregateException(exceptions));
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

            return tcs.Task;
        };

        for (var i = _pipelines.Count - 1; i >= 0; i--)
        {
            var pipeline = _pipelines[i];

            var old = next;
            next = (@event, c) => pipeline.OnEventAsync(old, @event, c);
        }

        return next(evt, ct);
    }
}
