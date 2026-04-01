using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using SmartAssist.Core.Models;

namespace SmartAssist.Infrastructure.Services;

public class IngestionMessageProducer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private const string QueueName = "document-ingestion";

    public IngestionMessageProducer(string hostName, string username, string password)
    {
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = username,
            Password = password
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false).GetAwaiter().GetResult();
    }

    public async Task PublishAsync(IngestionMessage message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true
        };

        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: QueueName,
            mandatory: false,
            basicProperties: properties,
            body: body);
    }

    public void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _connection?.CloseAsync().GetAwaiter().GetResult();
    }
}