using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(new ConnectionFactory { Uri = new Uri(Environment.GetEnvironmentVariable("RABBITMQ_URL") ?? "ERRO!!!") });

builder.Services.AddSingleton(new Configuracoes(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
