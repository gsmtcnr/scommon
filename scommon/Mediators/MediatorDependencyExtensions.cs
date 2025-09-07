using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace scommon;

public static class MediatorDependencyExtensions
{
    /// <summary>
    /// Configures the mediator into the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="config">An optional configurations action</param>
    /// <returns>The service collection after changes</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorOptions> config = null)
    {
        var options = new MediatorOptions(services);
        config?.Invoke(options);

        services.TryAdd(new ServiceDescriptor(typeof(IMediatorServiceProvider), typeof(MediatorServiceProvider), options.ServiceProviderLifetime));

        services.TryAdd(new ServiceDescriptor(
            typeof(IMediator), typeof(Mediator), options.Lifetime));

        services.TryAdd(new ServiceDescriptor(
            typeof(IFetcher<,>), typeof(Fetcher<,>), ServiceLifetime.Transient));
        services.TryAdd(new ServiceDescriptor(
            typeof(ISender<>), typeof(Sender<>), ServiceLifetime.Transient));
        services.TryAdd(new ServiceDescriptor(
            typeof(ISender<,>), typeof(Sender<,>), ServiceLifetime.Transient));
        services.TryAdd(new ServiceDescriptor(
            typeof(IBroadcaster<>), typeof(Broadcaster<>), ServiceLifetime.Transient));

        return services;
    }

    /// <summary>
    /// Configures the given type as a mediator <see cref="IPipeline"/>.
    /// Registration order will be kept when executing the pipeline.
    /// </summary>
    /// <typeparam name="T">The pipeline type</typeparam>
    /// <param name="options">The mediator options</param>
    /// <param name="lifetime">The pipeline lifetime</param>
    /// <returns>The options instance after changes</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static MediatorOptions AddPipeline<T>(this MediatorOptions options, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : class, IPipeline
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        options.Services.Add(new ServiceDescriptor(typeof(IPipeline), typeof(T), lifetime));
        return options;
    }

    /// <summary>
    /// Configures the given factory for mediator <see cref="IPipeline"/> instances.
    /// Registration order will be kept when executing the pipeline.
    /// </summary>
    /// <param name="options">The mediator options</param>
    /// <param name="factory">The pipeline factory</param>
    /// <param name="lifetime">The pipeline lifetime</param>
    /// <returns>The options instance after changes</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static MediatorOptions AddPipeline(
        this MediatorOptions options, Func<IServiceProvider, IPipeline> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        options.Services.Add(new ServiceDescriptor(typeof(IPipeline), factory, lifetime));
        return options;
    }

    /// <summary>
    /// Configures the given instance as a mediator <see cref="IPipeline"/>.
    /// Registration order will be kept when executing the pipeline.
    /// </summary>
    /// <typeparam name="T">The pipeline type</typeparam>
    /// <param name="options">The mediator options</param>
    /// <param name="instance">The singleton instance</param>
    /// <returns>The options instance after changes</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static MediatorOptions AddPipeline<T>(this MediatorOptions options, T instance)
        where T : class, IPipeline
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        options.Services.Add(new ServiceDescriptor(typeof(IPipeline), instance));
        return options;
    }

    /// <summary>
    /// Registers all the command, query and event handlers found in the given type assembly.
    /// </summary>
    /// <typeparam name="T">The type to scan the assembly</typeparam>
    /// <param name="options">The mediator options</param>
    /// <param name="lifetime">The handlers lifetime</param>
    /// <returns>The options instance after changes</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static MediatorOptions AddHandlersFromAssemblyOf<T>(this MediatorOptions options,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return options.AddHandlersFromAssembly(typeof(T).Assembly, lifetime);
    }

    /// <summary>
    /// Registers all the command, query and event handlers found in the given assembly.
    /// </summary>
    /// <param name="options">The mediator options</param>
    /// <param name="assembly">The assembly to scan</param>
    /// <param name="lifetime">The handlers lifetime</param>
    /// <returns>The options instance after changes</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static MediatorOptions AddHandlersFromAssembly(this MediatorOptions options,
        Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));

        var exportedTypes = assembly.ExportedTypes;
        foreach (var t in exportedTypes.Where(t => t.IsClass && !t.IsAbstract))
        {
            var implementedInterfaces = t.GetInterfaces();

            foreach (var i in implementedInterfaces.Where(e => e.IsGenericType))
            {
                var iGenericType = i.GetGenericTypeDefinition();
                if (iGenericType == typeof(ICommandHandler<>) ||
                    iGenericType == typeof(ICommandHandler<,>) ||
                    iGenericType == typeof(IEventHandler<>) ||
                    iGenericType == typeof(IQueryHandler<,>) ||
                    iGenericType == typeof(IValidationHandler<>))
                {
                    options.Services.Add(new ServiceDescriptor(i, t, lifetime));
                }
            }
        }

        return options;
    }
}
