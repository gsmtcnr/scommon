using Microsoft.EntityFrameworkCore;

namespace scommon_sample_web_api.Contexts;

public static class ContextExtensions
{
    // public static void AddSqlContext(this IServiceCollection serviceCollection, IConfiguration configuration)
    // {
    //     var connectionString = configuration.GetConnectionString("DbSQL")!;
    //     AddSqlContext(serviceCollection, connectionString);
    // }
    //
    // public static void AddSqlContext(this IServiceCollection serviceCollection, string connectionString)
    // {
    //     serviceCollection.AddDbContext<SampleContext>(options => { options.UseSqlServer(connectionString).EnableSensitiveDataLogging(false).LogTo(Console.WriteLine, LogLevel.Warning); }
    //     );
    // }
    public static void AddInMemoryContext(this IServiceCollection services)
    {
        services.AddDbContext<SampleContext>(options =>
        {
            options.UseInMemoryDatabase("sample_db");
        });
        
    }
}
