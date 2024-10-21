var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/transferencias", (SolicitacaoTransferencia solicitacaoTransferencia) =>
{
    return Results.Ok(new { transferenciaId = Guid.NewGuid() });
});

app.MapGet("/", () => "bacen ok");

app.Run();
