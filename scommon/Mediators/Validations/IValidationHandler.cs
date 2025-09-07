namespace scommon;

public interface IValidationHandler<in T> where T : class
    
{
    Task<IResultModel> ValidateAsync(T request, CancellationToken cancellationToken);
}