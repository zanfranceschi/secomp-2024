using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly NpgsqlDataSource _dbDataSource;
    private readonly IConnection _brokerConnection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private const string NOTIFICATION_EXCHANGE = "transferencias.realizada";
    private const string NOTIFICATION_QUEUE = "transferencias.realizada.ledger";

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
        _channel.BasicQos(0, 1, false);
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
            var persistenciaLedgerCmd = _dbDataSource.CreateCommand();
            persistenciaLedgerCmd.CommandText = @"insert into lancamentos_contabeis (operacao, cliente_id, debito, credito)
                                                values ($1, $2, $3, $4), ($5, $6, $7, $8)";
            persistenciaLedgerCmd.Parameters.AddWithValue("TRANSFERÊNCIA INTERBANCÁRIA");
            persistenciaLedgerCmd.Parameters.AddWithValue(evento.clienteIdDe);
            persistenciaLedgerCmd.Parameters.AddWithValue(evento.valor);
            persistenciaLedgerCmd.Parameters.AddWithValue(0.0m);

            persistenciaLedgerCmd.Parameters.AddWithValue("TRANSFERÊNCIA INTERBANCÁRIA");
            persistenciaLedgerCmd.Parameters.AddWithValue(evento.clienteIdPara);
            persistenciaLedgerCmd.Parameters.AddWithValue(0.0m);
            persistenciaLedgerCmd.Parameters.AddWithValue(evento.valor);

            var registrosAfetadosLedger = await persistenciaLedgerCmd.ExecuteNonQueryAsync();

            if (registrosAfetadosLedger != 2)
                throw new Exception("Algo errado não está certo. O número de registros afetados é diferente de 1 na inserção do ledger.");

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
