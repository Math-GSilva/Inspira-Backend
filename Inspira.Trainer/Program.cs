using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Inspira.Trainer.Services;
using Inspira.Trainer.Repositories;
using Inspira.Trainer.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                               ?? Environment.GetEnvironmentVariable("SqlConnectionString");

        services.AddDbContext<TrainingDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUsuarioPreferenciaRepository, UsuarioPreferenciaRepository>();
        services.AddScoped<TrainingService>();
    })
    .UseSerilog((context, services, loggerConfiguration) => {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext();
    })
    .Build();

host.Run();