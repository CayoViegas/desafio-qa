using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MinhasFinancas.Infrastructure.Data;
using MinhasFinancas.API.Controllers;

namespace BackendTests.Config;

public class CustomWebApplicationFactory : WebApplicationFactory<CategoriasController>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<MinhasFinancasDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<MinhasFinancasDbContext>(options =>
            {
                options.UseSqlite("DataSource=:memory:");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MinhasFinancasDbContext>();
            db.Database.OpenConnection(); 
            db.Database.EnsureCreated();
        });
    }
}