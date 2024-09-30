using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddNpgsqlDataSource(
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "ERRO DE CONNECTION STRING"
);

var bacenUrl = Environment.GetEnvironmentVariable("BACEN_API_URL") ?? "ERRO DE URL";

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter("default", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromSeconds(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));


var app = builder.Build();

app.MapPost("/", async (SolicitacaoTransferenciaRequest request, HttpClient httpClient, NpgsqlConnection conn) => 
{
    app.Logger.LogInformation(request.ToString());
    
    await using (conn)
    {
        await conn.OpenAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "select 1";
        var result = await cmd.ExecuteScalarAsync();
    }
    var response = await httpClient.PostAsJsonAsync(bacenUrl, new SolicitacaoTransferenciaRequestBacen(Guid.NewGuid(), Guid.NewGuid(), 10M));
    var responsePayload = await response.Content.ReadFromJsonAsync<SolicitacaoTransferenciaResponseBacen>();
    
    return Results.Created("/xpto", responsePayload);

}).RequireRateLimiting("default");

app.MapGet("/", () => "ok");

app.Run();
