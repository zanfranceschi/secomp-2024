using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class Worker
    : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConnection _brokerConnection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;

    private readonly NpgsqlConnection _dbConnection;

    public Worker(ILogger<Worker> logger, ConnectionFactory connectionFactory, NpgsqlConnection dbConnection)
    {
        _logger = logger;

        _dbConnection = dbConnection;

        _brokerConnection = connectionFactory.CreateConnection();
        _channel = _brokerConnection.CreateModel();
        _consumer = new EventingBasicConsumer(_channel);

        _channel.ExchangeDeclare("teste", ExchangeType.Topic);
        _channel.QueueDeclare("worker", true, false, false, null);
        _channel.QueueBind("worker", "teste", "*");

        _dbConnection.Open();

        _consumer.Received += async (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] {message}");

            await using var cmd = _dbConnection.CreateCommand();
            cmd.CommandText = "insert into codigos (valor) values ($1)";
            cmd.Parameters.AddWithValue($"broker: {message}");
            var affectedRRows = await cmd.ExecuteNonQueryAsync();

        };

        _channel.BasicConsume(queue: "worker",
                     autoAck: true,
                     consumer: _consumer);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information) && false)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
