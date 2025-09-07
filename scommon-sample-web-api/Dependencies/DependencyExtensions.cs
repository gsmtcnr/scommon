using scommon;

namespace scommon_sample_web_api.Dependencies;

public static class DependencyExtensions
{
    // public static void AddIoCManager(this IServiceCollection serviceCollection, WebApplicationBuilder builder)
    // {
    //     serviceCollection.AddSingleton<IDependencyManager>(x => ActivatorUtilities.CreateInstance<IoCManager>(x, builder.Services));
    // }
    public static void AddAllDependencies(this IServiceCollection serviceCollection)
    {
        serviceCollection.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(classes => classes.AssignableTo<ITransientDependency>()).AsImplementedInterfaces().WithTransientLifetime()
            .AddClasses(classes => classes.AssignableTo<IScopedDependency>()).AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo<ISingletonDependency>()).AsImplementedInterfaces().WithSingletonLifetime()
        );
    }
}
