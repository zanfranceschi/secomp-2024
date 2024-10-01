using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddConsole();

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

app.MapPost("/transferencias",
    async (SolicitacaoTransferenciaRequest request,
           HttpClient httpClient,
           NpgsqlConnection conn,
           ILogger<Api> logger) =>
{
    logger.LogInformation(request.ToString());

    SolicitacaoTransferenciaResponse response = new SolicitacaoTransferenciaResponse { status = TransferenciaStatus.Pendente };

    await using (conn)
    {
        await conn.OpenAsync();
        
        var mediaTransferenciaCmd = conn.CreateCommand();
        mediaTransferenciaCmd.CommandText = @"select coalesce(min(valor), 0) as min,
                                                     coalesce(avg(valor), 0) as avg,
                                                     coalesce(max(valor), 0) as max,
                                                     coalesce(sum(valor), 0) as sum
                                              from transferencias";
        var mediaValorTransferencia = await mediaTransferenciaCmd.ExecuteScalarAsync();

        logger.LogInformation("média de trasferência: {media}", mediaValorTransferencia);

        var bacenResponse = await httpClient.PostAsJsonAsync(bacenUrl,
            new SolicitacaoTransferenciaRequestBacen(request.clienteIdDe, request.clienteIdPara, request.valor));

        SolicitacaoTransferenciaResponseBacen responsePayload = await bacenResponse.Content.ReadFromJsonAsync<SolicitacaoTransferenciaResponseBacen>();
        response.transferenciaId = responsePayload.transferenciaId;
        response.status = TransferenciaStatus.Sucesso;

        var persistenciaTransferenciaCmd = conn.CreateCommand();
        persistenciaTransferenciaCmd.CommandText = "insert into transferencias (cliente_id_de, cliente_id_para, valor) values($1, $2, $3)";
        persistenciaTransferenciaCmd.Parameters.AddWithValue(request.clienteIdDe);
        persistenciaTransferenciaCmd.Parameters.AddWithValue(request.clienteIdPara);
        persistenciaTransferenciaCmd.Parameters.AddWithValue(request.valor);
        var registrosAfetados = await persistenciaTransferenciaCmd.ExecuteNonQueryAsync();

        if (registrosAfetados != 1)
            throw new Exception("Algo errado não está certo. O número de registros afetados é diferente de 1.");
    }

    return Results.Created($"/transferencias/${response.transferenciaId}",
        new
        {
            sucesso = true,
            href = $"/transferencias/{response.transferenciaId}"
        });

}).RequireRateLimiting("default");

app.MapGet("/", () => "ok");

app.Run();
