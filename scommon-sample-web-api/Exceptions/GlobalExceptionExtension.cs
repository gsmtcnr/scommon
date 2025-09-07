namespace scommon_sample_web_api.Exceptions;

public static class GlobalExceptionExtension
{
    public static void AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }
}
