using System.Text.RegularExpressions;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Npgsql;
using RabbitMQ.Client;
using System.Text;


Console.WriteLine(Environment.GetEnvironmentVariable("RABBITMQ_URL"));

var factory = new ConnectionFactory { Uri = new Uri(Environment.GetEnvironmentVariable("RABBITMQ_URL") ?? "ERRO!!!") };

var connection = factory.CreateConnection();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNpgsqlDataSource(
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "ERRO de connection string!!!"
);

builder.Services.AddSingleton(serviceProvider => connection.CreateModel());

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 1;
        options.Window = TimeSpan.FromSeconds(2);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));


var app = builder.Build();

app.UseRateLimiter();

app.MapGet("/", () => "Limited Hello World!").RequireRateLimiting("fixed");

app.MapPost("/", async (Codigo code, IModel channel, NpgsqlConnection conn, ILogger<Codigo> logger) =>
{
    var _ok = false;
    
    logger.LogInformation("Teste");

    await using (conn)
    {
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "insert into codigos (valor) values ($1)";
        cmd.Parameters.AddWithValue(code.Valor);
        var affectedRRows = await cmd.ExecuteNonQueryAsync();
        _ok = affectedRRows == 1;
    }

    
    channel.BasicPublish("teste", "*", null, Encoding.UTF8.GetBytes(code.Valor));


    var matches = Regex.Matches(code.Valor, @"([a-zA-Z]+)");
    var x = string.Join("", matches.Select((v, i) => v));
    return new { code.Valor, ok = _ok };

}); //.RequireRateLimiting("fixed");

app.Run();
