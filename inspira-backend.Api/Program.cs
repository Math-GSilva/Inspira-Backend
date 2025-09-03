using inspira_backend.Infra;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Pega a string de conex�o do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("InspiraDbConnection");

// 2. Adiciona o servi�o do DbContext ao container de inje��o de depend�ncia
builder.Services.AddDbContext<InspiraDbContext>(options =>
    options.UseNpgsql(connectionString, b =>
        b.MigrationsAssembly("inspira-backend.Infra")));


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
