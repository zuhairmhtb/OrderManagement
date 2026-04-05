
using Events.Dtos.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Subscriber;

namespace OrderManagement.Worker;

public class Program
{

    public static async Task Main(string[] args)
    {
        await new HostBuilder()
        .ConfigureAppConfiguration((context, config) =>
        {
            config
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true);
        })
        .ConfigureServices((context, services) =>
        {
            services.AddOptions();
            Database.DependencyInjection.Configure(services, context.Configuration, null);
            var rabbitMQConnectionString = context.Configuration.GetSection("RabbitMQ").Get<RabbitMQConfig>();
            Console.WriteLine($"RabbitMQ Connection String: {rabbitMQConnectionString}");
            if(rabbitMQConnectionString != null)
            {
                services.AddMassTransit(x =>
                {
                    x.AddConsumers(typeof(Program).Assembly);
                    x.SetJobConsumerOptions();
                    

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(
                            rabbitMQConnectionString.HostName, 
                            h => {  
                                h.Username(rabbitMQConnectionString.UserName);
                                h.Password(rabbitMQConnectionString.Password);
                            }
                        );
                        cfg.ConfigureEndpoints(context);
                    });
                });
            }

            services.AddHostedService<ConsoleApp>();
        })
        .Build()
        .RunAsync();
    }
}