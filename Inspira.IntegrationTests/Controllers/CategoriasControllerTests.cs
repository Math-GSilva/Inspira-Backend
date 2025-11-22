using FluentAssertions;
using inspira_backend.Application.DTOs;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Enums;
using inspira_backend.Infra;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace Inspira.IntegrationTests.Controllers
{
    public class CategoriasControllerTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFixture _fixture;

        public CategoriasControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.CreateClient();
        }

        private async Task AutenticarComoAdminAsync()
        {
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var adminUser = new RegisterRequestDto
            {
                CompleteName = "Admin Teste",
                Username = $"admin_{uniqueId}",
                Email = $"admin_{uniqueId}@teste.com",
                Password = "PasswordStrong123!",
                Role = UserRole.Administrador
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/register", adminUser);
            response.EnsureSuccessStatusCode();

            var loginResult = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResult.Token);
        }

        [Fact]
        public async Task GetAll_DeveRetornarListaDeCategorias()
        {
            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                context.Categorias.RemoveRange(context.Categorias); // Limpeza
                await context.SaveChangesAsync();

                await context.Categorias.AddRangeAsync(
                    new Categoria { Nome = "Aquarela", Descricao = "Técnica com água" },
                    new Categoria { Nome = "Óleo", Descricao = "Técnica a óleo" }
                );
                await context.SaveChangesAsync();
            }

            var response = await _client.GetAsync("/api/Categorias");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<CategoriaResponseDto>>();
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Nome == "Aquarela");
        }

        [Fact]
        public async Task GetById_QuandoExiste_DeveRetornarCategoria()
        {
            var id = Guid.NewGuid();
            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.Categorias.AddAsync(new Categoria { Id = id, Nome = "Digital" });
                await context.SaveChangesAsync();
            }

            var response = await _client.GetAsync($"/api/Categorias/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<CategoriaResponseDto>();
            result.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetById_QuandoNaoExiste_DeveRetornarNotFound()
        {
            var response = await _client.GetAsync($"/api/Categorias/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_QuandoAdmin_DeveCriarComSucesso()
        {
            await AutenticarComoAdminAsync();
            var dto = new CreateCategoriaDto { Nome = "Escultura 3D", Descricao = "Modelagem" };

            var response = await _client.PostAsJsonAsync("/api/Categorias", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<CategoriaResponseDto>();
            result.Nome.Should().Be(dto.Nome);
            result.Id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Create_QuandoNomeDuplicado_DeveRetornarBadRequest()
        {
            await AutenticarComoAdminAsync();

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.Categorias.AddAsync(new Categoria { Nome = "Duplicada" });
                await context.SaveChangesAsync();
            }

            var dto = new CreateCategoriaDto { Nome = "Duplicada", Descricao = "Tentando criar de novo" };

            var response = await _client.PostAsJsonAsync("/api/Categorias", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var errorMsg = await response.Content.ReadAsStringAsync();
            errorMsg.Should().Contain("Uma categoria com este nome já existe");
        }

        [Fact]
        public async Task Create_QuandoNaoAutenticado_DeveRetornarUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;
            var dto = new CreateCategoriaDto { Nome = "Hacker" };

            var response = await _client.PostAsJsonAsync("/api/Categorias", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Update_QuandoAdmin_DeveAtualizar()
        {
            await AutenticarComoAdminAsync();
            var id = Guid.NewGuid();

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.Categorias.AddAsync(new Categoria { Id = id, Nome = "Antiga" });
                await context.SaveChangesAsync();
            }

            var dto = new UpdateCategoriaDto { Nome = "Nova", Descricao = "Atualizada" };

            var response = await _client.PutAsJsonAsync($"/api/Categorias/{id}", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<CategoriaResponseDto>();
            result.Nome.Should().Be("Nova");
            result.Descricao.Should().Be("Atualizada");
        }

        [Fact]
        public async Task Update_QuandoIdNaoExiste_DeveRetornarNotFound()
        {
            await AutenticarComoAdminAsync();
            var dto = new UpdateCategoriaDto { Nome = "Fantasma" };

            var response = await _client.PutAsJsonAsync($"/api/Categorias/{Guid.NewGuid()}", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_QuandoAdmin_DeveRemover()
        {
            await AutenticarComoAdminAsync();
            var id = Guid.NewGuid();

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.Categorias.AddAsync(new Categoria { Id = id, Nome = "Para Deletar" });
                await context.SaveChangesAsync();
            }

            var response = await _client.DeleteAsync($"/api/Categorias/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                var exists = await context.Categorias.FindAsync(id);
                exists.Should().BeNull();
            }
        }

        [Fact]
        public async Task Delete_QuandoIdNaoExiste_DeveRetornarNotFound()
        {
            await AutenticarComoAdminAsync();

            var response = await _client.DeleteAsync($"/api/Categorias/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}