using Microsoft.EntityFrameworkCore;
using PrinzipTrackerTest.Models;
using PrinzipTrackerTest.Services;

namespace PrinzipTrackerTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();

            builder.Services.AddDbContext<PrinzipDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Singleton);

            builder.Services.AddHostedService<PriceUpdaterMonitoringService>();
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

                      
            app.MapControllers();

            app.Run();
        }
    }
}
