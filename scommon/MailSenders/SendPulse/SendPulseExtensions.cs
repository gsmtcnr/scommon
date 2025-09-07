using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace scommon.SendPulse;

public static class SendPulseExtensions
{
    public static void AddSendPulseMailSender(this IServiceCollection serviceCollection, IConfigurationSection section)
    {
        serviceCollection.Configure<SendPulseOptions>(section);
        serviceCollection.AddSingleton<IMailSender, SendPulseMailSender>();
    }
}
