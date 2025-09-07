using MassTransit;

namespace scommon;

public class RabbitQueueStructure : IQueueStructure
{
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly IBusControl _busControl;

    public RabbitQueueStructure(RabbitMqOptions rabbitMqOptions)
    {
        _rabbitMqOptions = rabbitMqOptions;
        _busControl = Bus.Factory.CreateUsingRabbitMq(configuration =>
        {
            configuration.Host(rabbitMqOptions.Uri, hostConfiguration =>
            {
                hostConfiguration.Username(rabbitMqOptions.UserName);
                hostConfiguration.Password(rabbitMqOptions.Password);
            });
        });
    }

    public Task ConsumerAsync<T>(string queueName, IConsumerHandler<T> consumerHandler) where T : class
    {
        try
        {
            var result = _busControl.ConnectReceiveEndpoint(queueName,
                e =>
                {
                    e.Consumer(() => consumerHandler);
                    e.UseRateLimit(999, TimeSpan.FromMinutes(1));
                    e.UseRetry(r => r.Interval(2, TimeSpan.FromSeconds(2)));
                    e.PrefetchCount = 32; // Kuyruktan aynı anda alınacak mesaj sayısı
                    e.ConcurrentMessageLimit = 10; // Aynı anda işlenecek mesaj sayısı
                    e.UseCircuitBreaker(cbConfiguration =>
                    {
                        cbConfiguration.TripThreshold = 25;
                        cbConfiguration.ActiveThreshold = 5;
                        cbConfiguration.TrackingPeriod = TimeSpan.FromMinutes(5);
                        cbConfiguration.ResetInterval = TimeSpan.FromMinutes(10);
                    });
                });
            Console.WriteLine($"Listening {queueName}... Press any key to exit.", queueName);
            if (result.Ready.IsCompleted)
            {
                result.Ready.Start();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return Task.CompletedTask;
    }

    public async Task SendAsync<T>(string queueName, T payload, CancellationToken ct)
        where T : class
    {
        var sendToUri = new Uri($"{_rabbitMqOptions.Uri}/{queueName}");
        var sendEndpoint = await _busControl.GetSendEndpoint(sendToUri);
        await sendEndpoint.Send<T>(payload, ct);
    }

    public Task PublishAsync<T>(T payload, CancellationToken ct)
        where T : class
    {
        return _busControl.Publish<T>(payload, ct);
    }
}
