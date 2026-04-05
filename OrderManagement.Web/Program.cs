
using Events.Dtos.Configuration;
using MassTransit;
using OrderManagement.Database;
using OrderManagement.Web.Interfaces;
using OrderManagement.Web.Services;

namespace OrderManagement.Web;

public class Program
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // Add logging dependencies
        services.AddLogging(config =>
        {
            config.AddConsole();
            // Add other logging providers as needed
            
        });
    }

    static void ConfigureBroker(IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMQConnectionString = configuration.GetSection("RabbitMQ").Get<RabbitMQConfig>();
        Console.WriteLine($"RabbitMQ Connection String: {rabbitMQConnectionString}");
        if(rabbitMQConnectionString != null)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(
                        rabbitMQConnectionString.HostName, 
                        h => {  
                            h.Username(rabbitMQConnectionString.UserName);
                            h.Password(rabbitMQConnectionString.Password);
                        }
                    );
                });
            });
        }
    }
    
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        DependencyInjection.Configure(builder.Services, builder.Configuration, typeof(Program).Assembly);
        ConfigureBroker(builder.Services, builder.Configuration);
        ConfigureServices(builder.Services);
        

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            // Configure Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "SampleWebApi API V1"); // Default endpoint for OpenAPI
                options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseRouting();
        app.MapControllers();
        app.Run();
    }
}
