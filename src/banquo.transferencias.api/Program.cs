using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddConsole();

var connectionFactory = new ConnectionFactory
{
    Uri = new Uri(Environment.GetEnvironmentVariable("RABBITMQ_URL")),
    ClientProvidedName = "banquo.transferencias.api"
};

var connection = connectionFactory.CreateConnection();

var exchange = "transferencias.realizar";
var channel = connection.CreateModel();
channel.ExchangeDeclare(exchange, ExchangeType.Topic, true, false, null);

builder.Services.AddSingleton(_ =>
{
    if (channel.IsOpen)
        return channel;

    channel = connection.CreateModel();
    return channel;
});

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter("default", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromSeconds(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));


var app = builder.Build();

app.MapPost("/transferencias",
    (SolicitacaoTransferenciaRequest request,
     IModel channel,
    ILogger<Api> _logger) =>
{
    var transferenciaId = Guid.NewGuid();

    RealizarTransferenciaCommand cmd = new RealizarTransferenciaCommand(transferenciaId, request.clienteIdDe, request.clienteIdPara, request.valor);

    var cmdWire = JsonSerializer.Serialize(cmd);

    channel.BasicPublish(exchange, "transferencias.realizar", null, Encoding.UTF8.GetBytes(cmdWire));

    return Results.Accepted($"/transferencias/${transferenciaId}",
        new
        {
            sucesso = true,
            href = $"/transferencias/{transferenciaId}"
        });

});

app.MapGet("/", () => "banquo transferências api - ok");

app.Run();
