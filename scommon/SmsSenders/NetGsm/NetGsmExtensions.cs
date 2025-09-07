using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace scommon;

public static class NetGsmExtensions
{
    public static void AddNetGsmSmsSender(this IServiceCollection serviceCollection, IConfigurationSection sectionOptions)
    {
        serviceCollection.Configure<NetGsmOptions>(sectionOptions);
        serviceCollection.AddSingleton<ISmsSender, NetGsmSmsSender>();
    }
}
