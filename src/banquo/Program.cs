using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddConsole();

builder.Services.AddHttpClient();

builder.Services.AddNpgsqlDataSource(
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? throw new Exception("ERRO DE CONNECTION STRING")
);

var bacenUrl = Environment.GetEnvironmentVariable("BACEN_API_URL") ?? throw new Exception("ERRO DE CONNECTION URL");

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

        // query apenas para gerar I/O e simular computação de negócio
        var mediaTransferenciaCmd = conn.CreateCommand();
        mediaTransferenciaCmd.CommandText = @"select coalesce(min(valor), 0) as min,
                                                     coalesce(avg(valor), 0) as avg,
                                                     coalesce(max(valor), 0) as max,
                                                     coalesce(sum(valor), 0) as sum
                                              from transferencias";
        await using (var statsValorTransferencia = await mediaTransferenciaCmd.ExecuteReaderAsync())
        {
            if (statsValorTransferencia.Read())
            {
                logger.LogInformation("trasferências: mínima...{valor}", statsValorTransferencia.GetDecimal(0));
                logger.LogInformation("trasferências: média....{valor}", statsValorTransferencia.GetDecimal(1));
                logger.LogInformation("trasferências: máxima...{valor}", statsValorTransferencia.GetDecimal(2));
                logger.LogInformation("trasferências: soma.....{valor}", statsValorTransferencia.GetDecimal(3));
            }
        }

        var bacenResponse = await httpClient.PostAsJsonAsync(bacenUrl,
         new SolicitacaoTransferenciaRequestBacen(request.clienteIdDe, request.clienteIdPara, request.valor));

        SolicitacaoTransferenciaResponseBacen responsePayload = await bacenResponse.Content.ReadFromJsonAsync<SolicitacaoTransferenciaResponseBacen>();
        response.transferenciaId = responsePayload.transferenciaId;
        response.status = TransferenciaStatus.Sucesso;

        await using var transaction = await conn.BeginTransactionAsync();

        var persistenciaTransferenciaCmd = conn.CreateCommand();
        persistenciaTransferenciaCmd.CommandText = "insert into transferencias (cliente_id_de, cliente_id_para, valor) values($1, $2, $3)";
        persistenciaTransferenciaCmd.Parameters.AddWithValue(request.clienteIdDe);
        persistenciaTransferenciaCmd.Parameters.AddWithValue(request.clienteIdPara);
        persistenciaTransferenciaCmd.Parameters.AddWithValue(request.valor);
        var registrosAfetadosTransferencia = await persistenciaTransferenciaCmd.ExecuteNonQueryAsync();

        if (registrosAfetadosTransferencia != 1)
            throw new Exception("Algo errado não está certo. O número de registros afetados é diferente de 1 na inserção da transferência.");

        var persistenciaLedgerCmd = conn.CreateCommand();
        persistenciaLedgerCmd.CommandText = @"insert into lancamentos_contabeis (operacao, cliente_id, debito, credito)
                                                values ($1, $2, $3, $4), ($5, $6, $7, $8)";
        persistenciaLedgerCmd.Parameters.AddWithValue("TRANSFERÊNCIA INTERBANCÁRIA");
        persistenciaLedgerCmd.Parameters.AddWithValue(request.clienteIdDe);
        persistenciaLedgerCmd.Parameters.AddWithValue(request.valor);
        persistenciaLedgerCmd.Parameters.AddWithValue(0.0m);

        persistenciaLedgerCmd.Parameters.AddWithValue("TRANSFERÊNCIA INTERBANCÁRIA");
        persistenciaLedgerCmd.Parameters.AddWithValue(request.clienteIdPara);
        persistenciaLedgerCmd.Parameters.AddWithValue(0.0m);
        persistenciaLedgerCmd.Parameters.AddWithValue(request.valor);

        var registrosAfetadosLedger = await persistenciaLedgerCmd.ExecuteNonQueryAsync();

        if (registrosAfetadosLedger != 2)
            throw new Exception("Algo errado não está certo. O número de registros afetados é diferente de 1 na inserção do ledger.");

        await transaction.CommitAsync();
    }

    return Results.Created($"/transferencias/${response.transferenciaId}",
        new
        {
            sucesso = true,
            href = $"/transferencias/{response.transferenciaId}"
        });

}).RequireRateLimiting("default");

app.MapGet("/", () => "banquo ok");

app.Run();
