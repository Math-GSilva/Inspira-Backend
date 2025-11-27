using inspira_backend.Application.Interfaces;
using inspira_backend.Infra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Inspira.IntegrationTests
{
    public class IntegrationTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("inspira_test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"JwtSettings:Secret", "chave_super_secreta_para_testes_integracao_123_tem_que_ser_grande"},
                    {"JwtSettings:Issuer", "InspiraTest"},
                    {"JwtSettings:Audience", "InspiraTest"},
                    {"JwtSettings:ExpiryMinutes", "60"},
                    
                    {"CloudinarySettings:CloudName", "cloud_fake"},
                    {"CloudinarySettings:ApiKey", "123456"},
                    {"CloudinarySettings:ApiSecret", "abcdef"},
                    {"Serilog:WriteTo:1:Args:connectionString", "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://localhost;LiveEndpoint=https://localhost"}
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<InspiraDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<InspiraDbContext>(options =>
                {
                    options.UseNpgsql(_dbContainer.GetConnectionString());
                });

                var uploadDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediaUploadService));
                if (uploadDescriptor != null) services.Remove(uploadDescriptor);

                services.AddScoped<IMediaUploadService, FakeMediaUploadService>();
            });
        }
    }
    public class FakeMediaUploadService : IMediaUploadService
    {
        public Task<string?> UploadAsync(IFormFile file)
        {
            return Task.FromResult($"http://fake-cloudinary.com/{Guid.NewGuid()}_{file.FileName}");
        }
    }
}