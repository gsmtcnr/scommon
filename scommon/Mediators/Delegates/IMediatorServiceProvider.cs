namespace scommon;

/// <summary>
/// The service provider for mediator dependencies
/// </summary>
public interface IMediatorServiceProvider
{
    /// <summary>
    /// Builds an instance for the given service type.
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The service instance or null if not found</returns>
    T BuildService<T>() where T : class;

    /// <summary>
    /// Builds a collection of instances for the given service type.
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The service instances or empty if none found</returns>
    IEnumerable<T> BuildServices<T>() where T : class;
}