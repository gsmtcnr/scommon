using Microsoft.Extensions.DependencyInjection;

namespace scommon;

public static class RedisPubSubExtensions
{
    public static void AddRedisPubSub(this IServiceCollection serviceCollection, string? connectionString, string? prefix)
    {
        var pubSub = new RedisPubSub(connectionString, prefix);

        serviceCollection.AddSingleton<IPubSub>(pubSub);
    }
}
