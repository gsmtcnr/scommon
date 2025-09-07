using Microsoft.Extensions.DependencyInjection;

namespace scommon;

public static class RabbitMqExtensions
{
    public static void AddRabbitQueueStructure(this IServiceCollection serviceCollection, RabbitMqOptions options)
    {
        var queueStructure = new RabbitQueueStructure(options);

        serviceCollection.AddSingleton<IQueueStructure>(queueStructure);
    }
}
