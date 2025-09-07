using FluentValidation;

namespace scommon_sample_web_api.Validations;

public static class ValidationExtensions
{
    public static void AddRequestValidators(this IServiceCollection services)
    {
        services.Scan(s => s.FromAssemblyOf<Program>()
            .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }
}
