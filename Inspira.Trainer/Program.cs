using Inspira.Trainer.Data;
using Inspira.Trainer.Repositories;
using Inspira.Trainer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");

                services.AddDbContext<TrainingDbContext>(options =>
                    options.UseNpgsql(connectionString));
                services.AddScoped<IUsuarioPreferenciaRepository, UsuarioPreferenciaRepository>();

                services.AddHostedService<TrainingService>();
            })
            .UseSerilog((context, loggerConfig) =>
                loggerConfig.ReadFrom.Configuration(context.Configuration))
            .Build();

        await host.RunAsync();
    }
}
