using Microsoft.Extensions.DependencyInjection;

namespace scommon;

public static class ValidationPipelineExtensions
{
    /// <summary>
    /// Registers a <see cref="ValidationPipeline"/>.
    /// </summary>
    /// <param name="options">The mediator registration options</param>
    /// <param name="config">Configures the pipeline options</param>
    /// <param name="lifetime">The pipeline lifetime</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static MediatorOptions AddPipelineForValidation(
        this MediatorOptions options, Action<ValidationPipelineOptions> config = null, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (config != null)
            options.Services.Configure(config);

        options.AddPipeline<ValidationPipeline>(lifetime);

        return options;
    }
}
