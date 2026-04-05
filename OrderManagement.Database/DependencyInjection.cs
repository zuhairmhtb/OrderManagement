using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Database.Context;

namespace OrderManagement.Database;

public static class DependencyInjection
{
    public static void Configure(this IServiceCollection services, IConfiguration configuration, Assembly? assembly)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("OrderManagement"), config =>
            {
                if(assembly != null) {
                    config.MigrationsAssembly(assembly.FullName);
                }
            });
        });
    }
}