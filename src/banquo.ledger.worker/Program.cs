using RabbitMQ.Client;
using Npgsql;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(new ConnectionFactory
{
    Uri = new Uri(Environment.GetEnvironmentVariable("RABBITMQ_URL")),
    ClientProvidedName = "banquo.ledger.worker"
});

builder.Services.AddNpgsqlDataSource(
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
