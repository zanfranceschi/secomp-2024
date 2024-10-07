using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddConsole();

builder.Services.AddNpgsqlDataSource(
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? throw new Exception("ERRO DE CONNECTION STRING")
);

var app = builder.Build();

app.MapGet("/limites/{clienteId}", async (Guid clienteId, NpgsqlConnection conn, ILogger<Api> logger) =>
{
    await using (conn)
    {
        await conn.OpenAsync();

        // query apenas para gerar I/O e simular computação de negócio
        var mediaTransferenciaCmd = conn.CreateCommand();
        mediaTransferenciaCmd.CommandText = @"select coalesce(min(valor::decimal), .0) as min,
                                                     coalesce(avg(valor::decimal), .0) as avg,
                                                     coalesce(max(valor::decimal), .0) as max,
                                                     coalesce(sum(valor::decimal), .0) as sum
                                              from transferencias;";

        await using var statsValorTransferencia = await mediaTransferenciaCmd.ExecuteReaderAsync();

        if (statsValorTransferencia.Read())
        {
            logger.LogInformation("trasferências: mínima...{valor}", statsValorTransferencia.GetDecimal(0));
            logger.LogInformation("trasferências: média....{valor}", statsValorTransferencia.GetDecimal(1));
            logger.LogInformation("trasferências: máxima...{valor}", statsValorTransferencia.GetDecimal(2));
            logger.LogInformation("trasferências: soma.....{valor}", statsValorTransferencia.GetDecimal(3));
        }

        return new
        {
            ValorAprovado = true,
            TransferenciaMinima = statsValorTransferencia.GetDecimal(0),
            TransferenciaMedia = statsValorTransferencia.GetDecimal(1),
            TransferenciaMaxima = statsValorTransferencia.GetDecimal(2),
            TransferenciasSoma = statsValorTransferencia.GetDecimal(3)
        };
    }

});

app.Run();
