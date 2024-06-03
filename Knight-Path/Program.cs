using Azure.Storage.Queues;
using KnightPath.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KnightPath.Services.Interfaces;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .Build();

var connectionString = config.GetSection("Settings")["AzureWebJobsStorage"];
var queueName = config.GetSection("Settings")["QueueName"];

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<IKnightService, KnightService>();

        services.AddAzureClients(builder =>
        {
            builder.AddClient<QueueClient, QueueClientOptions>((options, _, _) =>
            {
                options.MessageEncoding = QueueMessageEncoding.Base64;
                return new QueueClient(connectionString, queueName, options);
            });
        });
    })
    .Build();

host.Run();
