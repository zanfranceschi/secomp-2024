using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
public class Worker
    : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Configuracoes _configuracoes;
    private readonly IConnection _brokerConnection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private readonly HttpClient _httpClient;
    private readonly NpgsqlConnection _dbConnection;
    private const string COMMAND_EXCHANGE = "transferencias.realizar";
    private const string COMMAND_QUEUE = "transferencias.realizar.worker";
    private const string NOTIFICATION_EXCHANGE = "transferencias.realizada";

    private void DeclararObjetosRabbitMQ(ConnectionFactory connectionFactory)
    {
        using (var connection = connectionFactory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(COMMAND_EXCHANGE, ExchangeType.Topic, true, false, null);
            channel.QueueDeclare(COMMAND_QUEUE, true, false, false, null);
            channel.QueueBind(COMMAND_QUEUE, COMMAND_EXCHANGE, "#");
            channel.ExchangeDeclare(NOTIFICATION_EXCHANGE, ExchangeType.Topic);
        }
    }

    public Worker(ILogger<Worker> logger,
                  Configuracoes configuracoes,
                  ConnectionFactory connectionFactory,
                  HttpClient httpClient,
                  NpgsqlConnection dbConnection)
    {
        _logger = logger;
        _configuracoes = configuracoes;
        _httpClient = httpClient;

        DeclararObjetosRabbitMQ(connectionFactory);

        _dbConnection = dbConnection;

        _brokerConnection = connectionFactory.CreateConnection();
        _channel = _brokerConnection.CreateModel();
        _consumer = new EventingBasicConsumer(_channel);

        _dbConnection.Open();

        _consumer.Received += async (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var cmd = JsonSerializer.Deserialize<RealizarTransferenciaCommand>(message);

            var bacenHttpResponse = await httpClient.PostAsJsonAsync(_configuracoes.bacenUrl, new SolicitacaoTransferenciaRequestBacen(cmd.clienteIdDe, cmd.clienteIdPara, cmd.valor));
            SolicitacaoTransferenciaResponseBacen bacenResponse = await bacenHttpResponse.Content.ReadFromJsonAsync<SolicitacaoTransferenciaResponseBacen>();

            _logger.LogInformation(bacenResponse.ToString());

            var persistenciaTransferenciaCmd = _dbConnection.CreateCommand();
            persistenciaTransferenciaCmd.CommandText = "insert into transferencias (cliente_id_de, cliente_id_para, valor) values($1, $2, $3)";
            persistenciaTransferenciaCmd.Parameters.AddWithValue(cmd.clienteIdDe);
            persistenciaTransferenciaCmd.Parameters.AddWithValue(cmd.clienteIdPara);
            persistenciaTransferenciaCmd.Parameters.AddWithValue(cmd.valor);
            var registrosAfetadosTransferencia = await persistenciaTransferenciaCmd.ExecuteNonQueryAsync();
            if (registrosAfetadosTransferencia != 1)
            {
                _logger.LogError("Algo errado não está certo. O número de registros afetados é diferente de 1 na inserção da transferência.");
            }

            var notificacao = new TransferenciaRealizadaEvent(cmd.transferenciaId, cmd.clienteIdDe, cmd.clienteIdPara, cmd.valor);
            var notificaoWire = JsonSerializer.Serialize(notificacao);
            _channel.BasicPublish(NOTIFICATION_EXCHANGE, "transferencia.realizada", null, Encoding.UTF8.GetBytes(notificaoWire));

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(queue: COMMAND_QUEUE,
                              autoAck: false,
                              consumer: _consumer);
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
