using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace scommon;

public static class ContextTransactionPipelineExtensions
{
    /// <summary>
    /// Registers a <see cref="AddPipelineForContextTransaction{TDbContext}"/>.
    /// </summary>
    /// <param name="options">The mediator registration options</param>
    /// <param name="config">Configures the pipeline options</param>
    /// <param name="lifetime">The pipeline lifetime</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static MediatorOptions AddPipelineForContextTransaction<TDbContext>(
        this MediatorOptions options, Action<ContextTransactionPipelineOptions>? config = null, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TDbContext : DbContext
    {
        if (config != null)
            options.Services.Configure(config);

        options.AddPipeline<ContextTransactionPipeline<TDbContext>>(lifetime);

        return options;
    }
}
