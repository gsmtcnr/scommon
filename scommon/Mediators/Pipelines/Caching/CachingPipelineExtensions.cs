using Microsoft.Extensions.DependencyInjection;

namespace scommon;

public static class CachingPipelineExtensions
{
    /// <summary>
    /// Registers a <see cref="CachingPipeline"/>.
    /// </summary>
    /// <param name="options">The mediator registration options</param>
    /// <param name="config">Configures the pipeline options</param>
    /// <param name="lifetime">The pipeline lifetime</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static MediatorOptions AddPipelineForCaching(
        this MediatorOptions options, Action<CachingPipelineOptions>? config = null, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (config != null)
            options.Services.Configure(config);

        options.AddPipeline<CachingPipeline>(lifetime);

        return options;
    }
}
