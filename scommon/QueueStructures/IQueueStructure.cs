using MassTransit;

namespace scommon;

public interface IQueueStructure
{
    Task ConsumerAsync<T>(string queueName, IConsumerHandler<T> consumerHandler) where T : class;
    Task SendAsync<T>(string queueName, T payload, CancellationToken ct) where T : class;
    Task PublishAsync<T>(T payload, CancellationToken ct) where T : class;
}

public interface IConsumerHandler<in T> : IConsumer<T>
    where T : class
{
}

public interface IConsumerRegisterHandler : IScopedDependency
{
    Task RegisterAsync();
}