using FluentAssertions;
using inspira_backend.Application.DTOs;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Enums;
using inspira_backend.Infra;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Inspira.IntegrationTests.Controllers
{
    public class UsuarioControllerTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFixture _fixture;

        public UsuarioControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.CreateClient();
        }
        private async Task<(string Token, Guid UserId, string Username)> RegistrarEAutenticarAsync()
        {
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var registerDto = new RegisterRequestDto
            {
                CompleteName = $"User {uniqueId}",
                Username = $"user_{uniqueId}",
                Email = $"user_{uniqueId}@teste.com",
                Password = "PasswordStrong123!",
                Role = UserRole.Comum
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);
            response.EnsureSuccessStatusCode();

            var loginResult = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResult.Token);

            return (loginResult.Token, loginResult.UserId, loginResult.Username);
        }

        [Fact]
        public async Task GetProfile_QuandoUsuarioExiste_DeveRetornarDados()
        {
            var (_, _, username) = await RegistrarEAutenticarAsync();

            var response = await _client.GetAsync($"/api/Usuario/{username}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var profile = await response.Content.ReadFromJsonAsync<UsuarioProfileDto>();
            profile.Username.Should().Be(username);
        }

        [Fact]
        public async Task GetProfile_QuandoNaoExiste_DeveRetornarNotFound()
        {
            await RegistrarEAutenticarAsync();
            var response = await _client.GetAsync("/api/Usuario/usuario_fantasma_xyz");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Search_QuandoTermoValido_DeveRetornarLista()
        {
            await RegistrarEAutenticarAsync();

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.Usuarios.AddAsync(new Usuario
                {
                    NomeCompleto = "Alvo da Busca",
                    NomeUsuario = "alvo_busca",
                    Email = "alvo@teste.com",
                    SenhaHash = "hash"
                });
                await context.SaveChangesAsync();
            }

            var response = await _client.GetAsync("/api/Usuario/search?query=alvo");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var lista = await response.Content.ReadFromJsonAsync<List<UsuarioProfileDto>>();
            lista.Should().Contain(u => u.Username == "alvo_busca");
        }

        [Fact]
        public async Task Search_QuandoSemParametros_DeveRetornarBadRequest()
        {
            await RegistrarEAutenticarAsync();

            var response = await _client.GetAsync("/api/Usuario/search");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateMyProfile_QuandoAtualizarBio_DeveRetornarSucesso()
        {
            await RegistrarEAutenticarAsync();

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent("Bio atualizada via teste de integração"), "Bio");
            form.Add(new StringContent("https://meuportfolio.com"), "UrlPortifolio");

            var response = await _client.PutAsync("/api/Usuario/me", form);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var profile = await response.Content.ReadFromJsonAsync<UsuarioProfileDto>();
            profile.Bio.Should().Be("Bio atualizada via teste de integração");
            profile.UrlPortifolio.Should().Be("https://meuportfolio.com");
        }

        [Fact]
        public async Task Follow_QuandoSeguirUsuario_DeveRetornarSucesso()
        {
            var (_, seguidorId, _) = await RegistrarEAutenticarAsync();

            var idUsuarioB = Guid.NewGuid();
            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.Usuarios.AddAsync(new Usuario
                {
                    Id = idUsuarioB,
                    NomeUsuario = "usuario_b",
                    Email = "b@teste.com",
                    NomeCompleto = "B",
                    SenhaHash = "hash"
                });
                await context.SaveChangesAsync();
            }

            var response = await _client.PostAsync($"/api/Usuario/{idUsuarioB}/follow", null);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                var seguidor = await context.Seguidores.FindAsync(seguidorId, idUsuarioB);
                seguidor.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Follow_QuandoTentaSeguirSiMesmo_DeveRetornarBadRequest()
        {
            var (_, userId, _) = await RegistrarEAutenticarAsync();

            var response = await _client.PostAsync($"/api/Usuario/{userId}/follow", null);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Unfollow_QuandoJaSegue_DeveRemoverSeguidor()
        {
            var (_, seguidorId, _) = await RegistrarEAutenticarAsync();
            var idUsuarioB = Guid.NewGuid();

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                var usuarioB = new Usuario
                {
                    Id = idUsuarioB,
                    NomeUsuario = "usuario_b_unfollow",
                    Email = "b_unf@teste.com",
                    NomeCompleto = "B",
                    SenhaHash = "hash"
                };

                await context.Usuarios.AddAsync(usuarioB);
                await context.Seguidores.AddAsync(new Seguidor
                {
                    SeguidorId = seguidorId,
                    SeguidoId = idUsuarioB
                });
                await context.SaveChangesAsync();
            }

            var response = await _client.DeleteAsync($"/api/Usuario/{idUsuarioB}/follow");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                var seguidor = await context.Seguidores.FindAsync(seguidorId, idUsuarioB);
                seguidor.Should().BeNull();
            }
        }

        [Fact]
        public async Task Unfollow_QuandoNaoSegue_DeveRetornarBadRequest()
        {
            await RegistrarEAutenticarAsync();
            var idAleatorio = Guid.NewGuid();

            var response = await _client.DeleteAsync($"/api/Usuario/{idAleatorio}/follow");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}