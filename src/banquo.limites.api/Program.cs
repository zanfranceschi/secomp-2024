var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/limites/{clienteId}", () => new { ValorAprovado = true });

app.Run();
