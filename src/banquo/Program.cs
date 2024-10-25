using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddConsole();

builder.Services.AddHttpClient();

builder.Services.AddNpgsqlDataSource(
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
);

var bacenUrl = Environment.GetEnvironmentVariable("BACEN_API_URL");

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter("default", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromSeconds(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));

var app = builder.Build();

var consultarLimites = async (ILogger<Api> logger, NpgsqlConnection conn) =>
{
    if (conn.State != ConnectionState.Open)
        await conn.OpenAsync();

    /*
        Query para apenas gerar algum I/O e
        ficar propositalmente um pouco lento
        simulando alguma regra de negócio.
    */
    var statsTransferenciaCmd = conn.CreateCommand();
    statsTransferenciaCmd.CommandText = @"select coalesce(min(valor::decimal), .0) as min,
                                                 coalesce(avg(valor::decimal), .0) as avg,
                                                 coalesce(max(valor::decimal), .0) as max,
                                                 coalesce(sum(valor::decimal), .0) as sum
                                        from transferencias where realizada_em between $1 and $2;";
    var agora = DateTime.Now;
    var _10segundosAtras = agora.AddSeconds(-10);
    var _20segundosAtras = agora.AddSeconds(-20);
    statsTransferenciaCmd.Parameters.AddWithValue(_10segundosAtras);
    statsTransferenciaCmd.Parameters.AddWithValue(_20segundosAtras);

    await using (var statsValorTransferencia = await statsTransferenciaCmd.ExecuteReaderAsync())
    {
        if (statsValorTransferencia.Read())
        {
            logger.LogInformation("trasferências: mínima...{valor}", statsValorTransferencia.GetDecimal(0));
            logger.LogInformation("trasferências: média....{valor}", statsValorTransferencia.GetDecimal(1));
            logger.LogInformation("trasferências: máxima...{valor}", statsValorTransferencia.GetDecimal(2));
            logger.LogInformation("trasferências: soma.....{valor}", statsValorTransferencia.GetDecimal(3));
        }
    }

    return true;
};

var realizarTransferenciaBacen = async (SolicitacaoTransferenciaResponse response,
                                        SolicitacaoTransferenciaRequest request,
                                        HttpClient httpClient) =>
{
    var bacenResponse = await httpClient.PostAsJsonAsync(bacenUrl,
             new SolicitacaoTransferenciaRequestBacen(request.clienteIdDe, request.clienteIdPara, request.valor));

    SolicitacaoTransferenciaResponseBacen responsePayload = await bacenResponse.Content.ReadFromJsonAsync<SolicitacaoTransferenciaResponseBacen>();
    response.transferenciaId = responsePayload.transferenciaId;
    response.status = TransferenciaStatus.Sucesso;
};

var persistirTransferencia = async (NpgsqlConnection conn, SolicitacaoTransferenciaRequest request) =>
{
    var persistenciaTransferenciaCmd = conn.CreateCommand();
    persistenciaTransferenciaCmd.CommandText = "insert into transferencias (cliente_id_de, cliente_id_para, valor) values($1, $2, $3)";
    persistenciaTransferenciaCmd.Parameters.AddWithValue(request.clienteIdDe);
    persistenciaTransferenciaCmd.Parameters.AddWithValue(request.clienteIdPara);
    persistenciaTransferenciaCmd.Parameters.AddWithValue(request.valor);
    var registrosAfetadosTransferencia = await persistenciaTransferenciaCmd.ExecuteNonQueryAsync();

    if (registrosAfetadosTransferencia != 1)
        throw new Exception("Algo errado não está certo. O número de registros afetados é diferente de 1 na inserção da transferência.");
};

var persitirRegistrosLedger = async (NpgsqlConnection conn, SolicitacaoTransferenciaRequest request) =>
{
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
};

app.MapPost("/transferencias",
    async (SolicitacaoTransferenciaRequest request,
           HttpClient httpClient,
           NpgsqlDataSource dbDataSource,
           ILogger<Api> logger) =>
{
    SolicitacaoTransferenciaResponse response = new SolicitacaoTransferenciaResponse
    {
        status = TransferenciaStatus.Pendente
    };

    await using (var conn = dbDataSource.CreateConnection())
    {
        await conn.OpenAsync();
        await consultarLimites(logger, conn);
        await realizarTransferenciaBacen(response, request, httpClient);
        await using var transaction = await conn.BeginTransactionAsync();
        try
        {
            await persistirTransferencia(conn, request);
            await persitirRegistrosLedger(conn, request);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    return Results.Created($"/transferencias/${response.transferenciaId}",
        new
        {
            sucesso = true,
            href = $"/transferencias/{response.transferenciaId}"
        });

}); /*
        Se quiser adicionar rate limit para simular
        falhas mais facilmente, encadeie o método
        `.RequireRateLimiting("default");` a
        este método.
    */

app.MapGet("/", () => "banquo ok");

app.Run();
