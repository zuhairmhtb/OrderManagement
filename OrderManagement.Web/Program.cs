
using System.Text;
using Events.Dtos.Configuration;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Database;
using OrderManagement.Database.Dtos.Configuration;
using OrderManagement.Web.Interfaces;
using OrderManagement.Web.Services;

namespace OrderManagement.Web;

public class Program
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new Swashbuckle.AspNetCore.SwaggerGen.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Swashbuckle.AspNetCore.SwaggerGen.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Swashbuckle.AspNetCore.SwaggerGen.ParameterLocation.Header,
                Description = "Enter your JWT token. Example: Bearer {token}"
            });
            options.AddSecurityRequirement(new Swashbuckle.AspNetCore.SwaggerGen.OpenApiSecurityRequirement
            {
                {
                    new Swashbuckle.AspNetCore.SwaggerGen.OpenApiSecurityScheme
                    {
                        Reference = new Swashbuckle.AspNetCore.SwaggerGen.OpenApiReference
                        {
                            Type = Swashbuckle.AspNetCore.SwaggerGen.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // JWT Authentication
        var jwtConfig = configuration.GetSection("Jwt").Get<JwtConfig>()
            ?? throw new InvalidOperationException("JWT configuration is missing.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // Add logging dependencies
        services.AddLogging(config =>
        {
            config.AddConsole();
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
        ConfigureServices(builder.Services, builder.Configuration);
        

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
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
