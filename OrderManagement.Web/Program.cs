
using System.Text;
using Events.Dtos.Configuration;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OrderManagement.Database;
using OrderManagement.Database.Dtos.Configuration;
using OrderManagement.Web.Interfaces;
using OrderManagement.Web.Services;

namespace OrderManagement.Web;

public class Program
{
    public static void ConfigureJwt(IServiceCollection services, IConfigurationSection jwtSection) {
        var jwtConfig = jwtSection.Get<JwtConfig>()
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
    }
    public static void ConfigureServices(IServiceCollection services, IConfigurationSection jwtSection)
    {
        var jwtConfig = jwtSection.Get<JwtConfig>()
            ?? throw new InvalidOperationException("JWT configuration is missing.");
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

        var openApiInfo = new OpenApiInfo
        {
            Version = "v1",
            Title = "Order Management API",
            Description = "API for managing orders, customers, and products."
        };
        var scheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = jwtConfig.Scheme,
            BearerFormat = "JWT",
            Description = "Enter your JWT token without 'Bearer' keyword. Example: {token}"
        };
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((doc, context, cancellationToken) =>
            {
                doc.Info = openApiInfo;
                doc.Components ??= new OpenApiComponents();
                doc.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                doc.Components.SecuritySchemes.Add(jwtConfig.Scheme, scheme);

                doc.Security ??= new List<OpenApiSecurityRequirement>();
                doc.Security.Add(new OpenApiSecurityRequirement
                {
                    [ new OpenApiSecuritySchemeReference(jwtConfig.Scheme, doc) ] = []
                });

               return Task.CompletedTask; 
            });
        });
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", openApiInfo);
            options.AddSecurityDefinition(jwtConfig.Scheme, scheme);
            options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                [ new OpenApiSecuritySchemeReference(jwtConfig.Scheme, doc) ] = []
            });
        });
        services.AddEndpointsApiExplorer();
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
        var jwtConfig = builder.Configuration.GetSection("Jwt");
        if(jwtConfig == null)
        {
            throw new InvalidOperationException("JWT configuration section is missing in appsettings.");
        }
        builder.Services.Configure<JwtConfig>(jwtConfig);
        DependencyInjection.Configure(builder.Services, builder.Configuration, typeof(Program).Assembly);
        ConfigureBroker(builder.Services, builder.Configuration);
        ConfigureServices(builder.Services, jwtConfig);
        ConfigureJwt(builder.Services, jwtConfig);
        

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
