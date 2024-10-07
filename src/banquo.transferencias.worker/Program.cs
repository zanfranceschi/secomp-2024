using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddSingleton(new ConnectionFactory { Uri = new Uri(Environment.GetEnvironmentVariable("RABBITMQ_URL") ?? "ERRO!!!") });

builder.Services.AddSingleton(
    new Configuracoes(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"),
                      Environment.GetEnvironmentVariable("LIMITES_API_URL"),
                      Environment.GetEnvironmentVariable("BACEN_API_URL")));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
