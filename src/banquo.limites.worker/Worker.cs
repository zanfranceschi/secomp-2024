using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConnection _brokerConnection;
    private readonly NpgsqlDataSource _dbDataSource;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private const string NOTIFICATION_EXCHANGE = "transferencias.realizada";
    private const string NOTIFICATION_QUEUE = "transferencias.realizada.limites";

    private void DeclararObjetosRabbitMQ(ConnectionFactory connectionFactory)
    {
        using (var connection = connectionFactory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(NOTIFICATION_EXCHANGE, ExchangeType.Topic, true, false, null);
            channel.QueueDeclare(NOTIFICATION_QUEUE, true, false, false, null);
            channel.QueueBind(NOTIFICATION_QUEUE, NOTIFICATION_EXCHANGE, "#");
        }
    }

    public Worker(ILogger<Worker> logger,
                  NpgsqlDataSource dbDataSource,
                  ConnectionFactory connectionFactory)
    {
        _logger = logger;

        _dbDataSource = dbDataSource;

        DeclararObjetosRabbitMQ(connectionFactory);

        _brokerConnection = connectionFactory.CreateConnection();
        _channel = _brokerConnection.CreateModel();
        _consumer = new EventingBasicConsumer(_channel);
        _consumer.Received += OnMessageReceived;
        _channel.BasicQos(0, 200, false);
        _channel.BasicConsume(queue: NOTIFICATION_QUEUE,
                              autoAck: false,
                              consumer: _consumer);
    }

    protected async void OnMessageReceived(object? model, BasicDeliverEventArgs ea)
    {
        try
        {
            // recebe e desserializa mensagem
            byte[] body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var evento = JsonSerializer.Deserialize<TransferenciaRealizadaEvent>(message);

            // persiste localmente a transferência realizada
            var persistenciaTransferenciaCmd = _dbDataSource.CreateCommand();
            persistenciaTransferenciaCmd.CommandText = "insert into transferencias (bacen_transferencia_id, cliente_id_de, cliente_id_para, valor) values($1, $2, $3, $4)";
            persistenciaTransferenciaCmd.Parameters.AddWithValue(evento.transferenciaId);
            persistenciaTransferenciaCmd.Parameters.AddWithValue(evento.clienteIdDe);
            persistenciaTransferenciaCmd.Parameters.AddWithValue(evento.clienteIdPara);
            persistenciaTransferenciaCmd.Parameters.AddWithValue(evento.valor);
            var registrosAfetadosTransferencia = await persistenciaTransferenciaCmd.ExecuteNonQueryAsync();
            if (registrosAfetadosTransferencia != 1)
                _logger.LogError("Algo errado não está certo. O número de registros afetados é diferente de 1 na inserção da transferência.");

            _channel.BasicAck(ea.DeliveryTag, false);
            _logger.LogInformation("mensagem processada: {mensagem}", evento);
        }
        catch (Exception ex)
        {
            _channel.BasicNack(ea.DeliveryTag, false, true);
            _logger.LogError(ex, "Erro ao processar mensagem");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(10000, stoppingToken);
        }
    }
}
