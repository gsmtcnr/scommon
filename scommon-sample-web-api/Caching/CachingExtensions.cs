using scommon;

namespace scommon_sample_web_api.Caching;

public static class CachingExtensions
{
    public static void AddCachingServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICache, MemoryCache>();
    }
}
