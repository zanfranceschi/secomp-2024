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
    private readonly NpgsqlDataSource _dbDataSource;
    private readonly IConnection _brokerConnection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private readonly HttpClient _httpClient;
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
            channel.ExchangeDeclare(NOTIFICATION_EXCHANGE, ExchangeType.Topic, true, false, null);
        }
    }

    public Worker(ILogger<Worker> logger,
                  Configuracoes configuracoes,
                  NpgsqlDataSource dbDataSource,
                  ConnectionFactory connectionFactory,
                  HttpClient httpClient)
    {
        _logger = logger;
        _configuracoes = configuracoes;
        _dbDataSource = dbDataSource;
        _httpClient = httpClient;

        DeclararObjetosRabbitMQ(connectionFactory);

        _brokerConnection = connectionFactory.CreateConnection();
        _channel = _brokerConnection.CreateModel();
        _consumer = new EventingBasicConsumer(_channel);
        _consumer.Received += OnMessageReceived;
        _channel.BasicQos(0, 100, false);
        _channel.BasicConsume(queue: COMMAND_QUEUE,
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
            var cmd = JsonSerializer.Deserialize<RealizarTransferenciaCommand>(message);

            // consulta limites
            LimiteResponseBacen limiteHttpResponse = await _httpClient.GetFromJsonAsync<LimiteResponseBacen>(_configuracoes.LimitesUrl(cmd.clienteIdDe));
            if (!limiteHttpResponse.ValorAprovado)
                _logger.LogError("Oh, no! O lmite não foi aprovado!!! Foda-se, vou continuar mesmo assim.");

            // requisita o bacen para transferência
            var bacenHttpResponse = await _httpClient.PostAsJsonAsync(_configuracoes.BacenUrl, new SolicitacaoTransferenciaRequestBacen(cmd.clienteIdDe, cmd.clienteIdPara, cmd.valor));
            SolicitacaoTransferenciaResponseBacen bacenResponse = await bacenHttpResponse.Content.ReadFromJsonAsync<SolicitacaoTransferenciaResponseBacen>();

            // persiste localmente a transferência
            var persistenciaTransferenciaCmd = _dbDataSource.CreateCommand();
            persistenciaTransferenciaCmd.CommandText = "insert into transferencias (bacen_transferencia_id, cliente_id_de, cliente_id_para, valor) values($1, $2, $3, $4)";
            persistenciaTransferenciaCmd.Parameters.AddWithValue(bacenResponse.transferenciaId);
            persistenciaTransferenciaCmd.Parameters.AddWithValue(cmd.clienteIdDe);
            persistenciaTransferenciaCmd.Parameters.AddWithValue(cmd.clienteIdPara);
            persistenciaTransferenciaCmd.Parameters.AddWithValue(cmd.valor);
            var registrosAfetadosTransferencia = await persistenciaTransferenciaCmd.ExecuteNonQueryAsync();
            if (registrosAfetadosTransferencia != 1)
                _logger.LogError("Algo errado não está certo. O número de registros afetados é diferente de 1 na inserção da transferência.");

            // publica evento "transferência realizada"
            var notificacao = new TransferenciaRealizadaEvent(cmd.transferenciaId, bacenResponse.transferenciaId, cmd.clienteIdDe, cmd.clienteIdPara, cmd.valor);
            var notificaoWire = JsonSerializer.Serialize(notificacao);
            _channel.BasicPublish(NOTIFICATION_EXCHANGE, "transferencia.realizada", null, Encoding.UTF8.GetBytes(notificaoWire));

            _channel.BasicAck(ea.DeliveryTag, false);

            _logger.LogInformation("mensagem processada: {mensagem}", cmd);

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
