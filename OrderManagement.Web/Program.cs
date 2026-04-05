
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
        services.AddAutoMapper(options =>
        {
            // options.CreateMap<Student, CreateStudentCommand>().ReverseMap();
            // options.CreateMap<Classroom, CreateClassroomCommand>().ReverseMap();

        });

        // services.AddScoped<AppDbContext>();
    }
    
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
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
