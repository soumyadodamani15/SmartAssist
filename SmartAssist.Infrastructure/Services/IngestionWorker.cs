using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartAssist.Core.Models;

namespace SmartAssist.Infrastructure.Services;

public class IngestionWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IngestionWorker> _logger;
    private readonly string _hostName;
    private readonly string _username;
    private readonly string _password;
    private const string QueueName = "document-ingestion";

    public IngestionWorker(
        IServiceProvider serviceProvider,
        ILogger<IngestionWorker> logger,
        string hostName,
        string username,
        string password)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _hostName = hostName;
        _username = username;
        _password = password;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            UserName = _username,
            Password = _password
        };

        var connection = await factory.CreateConnectionAsync(stoppingToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<IngestionMessage>(json);

            if (message is null)
            {
                await channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                return;
            }

            _logger.LogInformation(
                "Processing document: {Title}", message.Title);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var ingestionService = scope.ServiceProvider
                    .GetRequiredService<DocumentIngestionService>();

                await ingestionService.IngestAsync(
                    message.Title,
                    message.Content,
                    message.ContentType);

                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                _logger.LogInformation(
                    "Document processed successfully: {Title}", message.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process document: {Title}", message.Title);
                await channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}