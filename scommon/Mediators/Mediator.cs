using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace scommon;

/// <summary>
/// Mediator to publish commands, broadcast events and fetch queries
/// </summary>
public class Mediator : IMediator
{
    private static readonly ConcurrentDictionary<Type, Func<IMediatorServiceProvider, object, CancellationToken, object>> ExpressionCache;
    private static readonly MethodInfo MethodServiceProviderBuildService;

    static Mediator()
    {
        ExpressionCache = new ConcurrentDictionary<Type, Func<IMediatorServiceProvider, object, CancellationToken, object>>();

        MethodServiceProviderBuildService = GetMethods(typeof(IMediatorServiceProvider))
            .Single(m => m.Name == nameof(IMediatorServiceProvider.BuildService));
    }

    private readonly IMediatorServiceProvider _serviceProvider;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="serviceProvider">The handler factory</param>
    /// <exception cref="ArgumentNullException"></exception>
    public Mediator(IMediatorServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public Task SendAsync(ICommand cmd, CancellationToken ct)
    {
        if (cmd == null) throw new ArgumentNullException(nameof(cmd));

        //Func<IMediatorServiceProvider, object, CancellationToken, object> caller = (provider, obj, cancellationToken) =>
        //{
        //    return provider
        //        .BuildService<ISender<TCommand>>()
        //        .SendAsync((TCommand) obj, cancellationToken);
        //};
        

        var caller = ExpressionCache.GetOrAdd(cmd.GetType(), commandType =>
        {
            var senderType = typeof(ISender<>).MakeGenericType(commandType);

            var mediatorServiceProviderParameter = Expression.Parameter(typeof(IMediatorServiceProvider));
            var commandParameter = Expression.Parameter(typeof(object));
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken));

            return Expression.Lambda<Func<IMediatorServiceProvider, object, CancellationToken, object>>(
                Expression.Call(
                    Expression.Call(
                        mediatorServiceProviderParameter,
                        MethodServiceProviderBuildService.MakeGenericMethod(senderType)
                    ),
                    GetMethods(senderType).Single(m => m.Name == "SendAsync"),
                    Expression.TypeAs(commandParameter, commandType),
                    cancellationTokenParameter
                ),
                mediatorServiceProviderParameter,
                commandParameter,
                cancellationTokenParameter
            ).Compile();
        });

        return (Task)caller(_serviceProvider, cmd, ct);
    }

    /// <inheritdoc />
    public Task<TResult> SendAsync<TResult>(ICommand<TResult> cmd, CancellationToken ct)
    {
        if (cmd == null) throw new ArgumentNullException(nameof(cmd));

        //Func<IMediatorServiceProvider, object, CancellationToken, object> caller = (provider, obj, cancellationToken) =>
        //{
        //    return provider
        //        .BuildService<ISender<TCommand<TResult>, TResult>>()
        //        .SendAsync((TCommand<TResult>) obj, cancellationToken);
        //};

        var caller = ExpressionCache.GetOrAdd(cmd.GetType(), commandType =>
        {
            var interfaces = commandType.GetInterfaces();
            var returnType = interfaces
                .Single(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(ICommand<>)
                ).GenericTypeArguments[0];

            var senderType = typeof(ISender<,>).MakeGenericType(commandType, returnType);

            var mediatorServiceProviderParameter = Expression.Parameter(typeof(IMediatorServiceProvider));
            var commandParameter = Expression.Parameter(typeof(object));
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken));

            return Expression.Lambda<Func<IMediatorServiceProvider, object, CancellationToken, object>>(
                Expression.Call(
                    Expression.Call(
                        mediatorServiceProviderParameter,
                        MethodServiceProviderBuildService.MakeGenericMethod(senderType)
                    ),
                    GetMethods(senderType).Single(m => m.Name == "SendAsync"),
                    Expression.TypeAs(commandParameter, commandType),
                    cancellationTokenParameter
                ),
                mediatorServiceProviderParameter,
                commandParameter,
                cancellationTokenParameter
            ).Compile();
        });

        return (Task<TResult>)caller(_serviceProvider, cmd, ct);
    }

    /// <inheritdoc />
    public Task BroadcastAsync(IEvent evt, CancellationToken ct)
    {
        if (evt == null) throw new ArgumentNullException(nameof(evt));

        //Func<IMediatorServiceProvider, object, CancellationToken, object> caller = (provider, obj, cancellationToken) =>
        //{
        //    return provider
        //        .BuildService<IBroadcaster<TEvent>>()
        //        .BroadcastAsync((TEvent) obj, cancellationToken);
        //};

        var caller = ExpressionCache.GetOrAdd(evt.GetType(), eventType =>
        {
            var broadcasterType = typeof(IBroadcaster<>).MakeGenericType(eventType);

            var mediatorServiceProviderParameter = Expression.Parameter(typeof(IMediatorServiceProvider));
            var eventParameter = Expression.Parameter(typeof(object));
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken));

            return Expression.Lambda<Func<IMediatorServiceProvider, object, CancellationToken, object>>(
                Expression.Call(
                    Expression.Call(
                        mediatorServiceProviderParameter,
                        MethodServiceProviderBuildService.MakeGenericMethod(broadcasterType)
                    ),
                    GetMethods(broadcasterType).Single(m => m.Name == "BroadcastAsync"),
                    Expression.TypeAs(eventParameter, eventType),
                    cancellationTokenParameter
                ),
                mediatorServiceProviderParameter,
                eventParameter,
                cancellationTokenParameter
            ).Compile();
        });

        return (Task)caller(_serviceProvider, evt, ct);
    }

    /// <inheritdoc />
    public Task<TResult> FetchAsync<TResult>(IQuery<TResult> query, CancellationToken ct)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        

        var caller = ExpressionCache.GetOrAdd(query.GetType(), queryType =>
        {
            var interfaces = queryType.GetInterfaces();
            var returnType = interfaces
                .Single(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IQuery<>)
                ).GenericTypeArguments[0];


            var fetcherType = typeof(IFetcher<,>).MakeGenericType(queryType, returnType);

            var mediatorServiceProviderParameter = Expression.Parameter(typeof(IMediatorServiceProvider));
            var queryParameter = Expression.Parameter(typeof(object));
            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken));

            return Expression.Lambda<Func<IMediatorServiceProvider, object, CancellationToken, object>>(
                Expression.Call(
                    Expression.Call(
                        mediatorServiceProviderParameter,
                        MethodServiceProviderBuildService.MakeGenericMethod(fetcherType)
                    ),
                    GetMethods(fetcherType).Single(m => m.Name == "FetchAsync"),
                    Expression.TypeAs(queryParameter, queryType),
                    cancellationTokenParameter
                ),
                mediatorServiceProviderParameter,
                queryParameter,
                cancellationTokenParameter
            ).Compile();
        });

        return (Task<TResult>)caller(_serviceProvider, query, ct);
    }

    private static IEnumerable<MethodInfo> GetMethods(Type type)
    {
        return type.GetMethods();
    }
}
