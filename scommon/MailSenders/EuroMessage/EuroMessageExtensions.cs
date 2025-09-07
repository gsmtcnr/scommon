using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace scommon;

public static class EuroMessageExtensions
{
    public static void AddEuroMessageMailSender(this IServiceCollection serviceCollection, IConfigurationSection section)
    {
        serviceCollection.Configure<EuroMessageOptions>(section);
        serviceCollection.AddSingleton<IMailSender, EuroMessageMailSender>();
    }
}
