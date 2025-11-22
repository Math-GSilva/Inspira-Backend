using FluentAssertions;
using inspira_backend.Application.DTOs;
using inspira_backend.Domain.Entities;
using inspira_backend.Infra;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Inspira.IntegrationTests.Controllers
{
    public class SeguidoresControllerTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFixture _fixture;

        public SeguidoresControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task GetSeguidores_QuandoUsuarioExiste_DeveRetornarListaDeSeguidores()
        {
            var idUsuarioAlvo = Guid.NewGuid();
            var idSeguidor1 = Guid.NewGuid();
            var idSeguidor2 = Guid.NewGuid();

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();

                await context.Usuarios.AddAsync(new Usuario
                {
                    Id = idUsuarioAlvo,
                    NomeUsuario = "famoso",
                    Email = "famoso@teste.com",
                    NomeCompleto = "O Famoso",
                    SenhaHash = "hash"
                });

                await context.Usuarios.AddRangeAsync(
                    new Usuario { Id = idSeguidor1, NomeUsuario = "fa1", Email = "fa1@teste.com", NomeCompleto = "Fã 1", SenhaHash = "hash" },
                    new Usuario { Id = idSeguidor2, NomeUsuario = "fa2", Email = "fa2@teste.com", NomeCompleto = "Fã 2", SenhaHash = "hash" }
                );

                await context.Seguidores.AddRangeAsync(
                    new Seguidor { SeguidorId = idSeguidor1, SeguidoId = idUsuarioAlvo },
                    new Seguidor { SeguidorId = idSeguidor2, SeguidoId = idUsuarioAlvo }
                );

                await context.SaveChangesAsync();
            }

            var response = await _client.GetAsync($"/api/usuarios/{idUsuarioAlvo}/seguidores");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var lista = await response.Content.ReadFromJsonAsync<List<SeguidorResumoDto>>();
            lista.Should().HaveCount(2);
            lista.Should().Contain(s => s.Username == "fa1");
            lista.Should().Contain(s => s.Username == "fa2");
        }

        [Fact]
        public async Task GetSeguidores_QuandoUsuarioNaoExiste_DeveRetornarNotFound()
        {
            var response = await _client.GetAsync($"/api/usuarios/{Guid.NewGuid()}/seguidores");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetSeguidores_QuandoUsuarioExisteMasNaoTemSeguidores_DeveRetornarListaVazia()
        {
            var idSolitario = Guid.NewGuid();
            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.Usuarios.AddAsync(new Usuario
                {
                    Id = idSolitario,
                    NomeUsuario = "solitario1",
                    Email = "sol1@teste.com",
                    NomeCompleto = "Só",
                    SenhaHash = "hash"
                });
                await context.SaveChangesAsync();
            }

            var response = await _client.GetAsync($"/api/usuarios/{idSolitario}/seguidores");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var lista = await response.Content.ReadFromJsonAsync<List<SeguidorResumoDto>>();
            lista.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSeguindo_QuandoUsuarioExiste_DeveRetornarQuemEleSegue()
        {
            var idUsuarioOrigem = Guid.NewGuid();
            var idSeguido1 = Guid.NewGuid();

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();

                await context.Usuarios.AddAsync(new Usuario
                {
                    Id = idUsuarioOrigem,
                    NomeUsuario = "stalker",
                    Email = "stalker@teste.com",
                    NomeCompleto = "Stalker",
                    SenhaHash = "hash"
                });

                await context.Usuarios.AddAsync(new Usuario
                {
                    Id = idSeguido1,
                    NomeUsuario = "alvo_stalker",
                    Email = "alvo_s@teste.com",
                    NomeCompleto = "Alvo",
                    SenhaHash = "hash"
                });

                await context.Seguidores.AddAsync(new Seguidor { SeguidorId = idUsuarioOrigem, SeguidoId = idSeguido1 });
                await context.SaveChangesAsync();
            }

            var response = await _client.GetAsync($"/api/usuarios/{idUsuarioOrigem}/seguindo");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var lista = await response.Content.ReadFromJsonAsync<List<SeguidorResumoDto>>();
            lista.Should().HaveCount(1);
            lista.First().Username.Should().Be("alvo_stalker");
        }

        [Fact]
        public async Task GetSeguindo_QuandoUsuarioNaoExiste_DeveRetornarNotFound()
        {
            var response = await _client.GetAsync($"/api/usuarios/{Guid.NewGuid()}/seguindo");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}