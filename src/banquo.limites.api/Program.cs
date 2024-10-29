using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddConsole();

builder.Services.AddNpgsqlDataSource(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"));

var app = builder.Build();

app.MapGet("/limites/{clienteId}", async (
    Guid clienteId,
    NpgsqlConnection conn,
    ILogger<Api> logger) =>
{
    try
    {
        await using (conn)
        {
            await conn.OpenAsync();

            /*
                Query para gerar I/O e processamento no banco para
                ficar propositalmente lento simulando alguma regra
                pesada de negócio.
            */
            var statsTransferenciaCmd = conn.CreateCommand();
            statsTransferenciaCmd.CommandText = @"select coalesce(min(valor::decimal), .0) as min,
                                                         coalesce(avg(valor::decimal), .0) as avg,
                                                         coalesce(max(valor::decimal), .0) as max,
                                                         coalesce(sum(valor::decimal), .0) as sum
                                                  from transferencias";
            
            await using var statsValorTransferencia = await statsTransferenciaCmd.ExecuteReaderAsync();

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
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Deu ruim");

        return new
        {
            ValorAprovado = true,
            TransferenciaMinima = 0M,
            TransferenciaMedia = 0M,
            TransferenciaMaxima = 0M,
            TransferenciasSoma = 0M
        };
    }

});

app.Run();
