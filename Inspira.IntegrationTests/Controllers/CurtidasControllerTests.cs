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
    public class CurtidasControllerTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFixture _fixture;

        public CurtidasControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.CreateClient();
        }
        private async Task<(Guid UserId, string Token)> RegistrarEAutenticarAsync()
        {
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var registerDto = new RegisterRequestDto
            {
                CompleteName = $"Fã {uniqueId}",
                Username = $"fan_{uniqueId}",
                Email = $"fan_{uniqueId}@teste.com",
                Password = "PasswordStrong123!",
                Role = UserRole.Comum
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);
            response.EnsureSuccessStatusCode();

            var loginResult = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResult.Token);

            return (loginResult.UserId, loginResult.Token);
        }
        private async Task<Guid> CriarObraParaTesteAsync()
        {
            using var scope = _fixture.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();

            var autor = new Usuario
            {
                NomeUsuario = $"autor_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Email = $"{Guid.NewGuid()}@art.com",
                NomeCompleto = "Artista",
                SenhaHash = "hash"
            };
            var categoria = new Categoria { Nome = $"Cat {Guid.NewGuid()}" };

            await context.Usuarios.AddAsync(autor);
            await context.Categorias.AddAsync(categoria);
            await context.SaveChangesAsync();

            var obra = new ObraDeArte
            {
                Titulo = "Obra para Curtir",
                Descricao = "Teste de Likes",
                UsuarioId = autor.Id,
                CategoriaId = categoria.Id,
                DataPublicacao = DateTime.UtcNow,
                UrlMidia = "http://fake.url/img.png",
                TipoConteudoMidia = "image/png"
            };

            await context.ObrasDeArte.AddAsync(obra);
            await context.SaveChangesAsync();

            return obra.Id;
        }

        [Fact]
        public async Task Curtir_QuandoObraExiste_DeveRetornarSucessoEIncrementarContagem()
        {
            await RegistrarEAutenticarAsync();
            var obraId = await CriarObraParaTesteAsync();
            var dto = new CreateCurtidaDto { ObraDeArteId = obraId };

            var response = await _client.PostAsJsonAsync("/api/Curtidas", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var status = await response.Content.ReadFromJsonAsync<CurtidaStatusDto>();
            status.Curtiu.Should().BeTrue();
            status.TotalCurtidas.Should().Be(1);
        }

        [Fact]
        public async Task Curtir_QuandoObraNaoExiste_DeveRetornarNotFound()
        {
            await RegistrarEAutenticarAsync();
            var dto = new CreateCurtidaDto { ObraDeArteId = Guid.NewGuid() };

            var response = await _client.PostAsJsonAsync("/api/Curtidas", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Curtir_QuandoNaoAutenticado_DeveRetornarUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;
            var dto = new CreateCurtidaDto { ObraDeArteId = Guid.NewGuid() };

            var response = await _client.PostAsJsonAsync("/api/Curtidas", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Curtir_QuandoJaCurtiu_NaoDeveDuplicarContagem()
        {
            var (userId, _) = await RegistrarEAutenticarAsync();
            var obraId = await CriarObraParaTesteAsync();

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.Curtidas.AddAsync(new Curtida { UsuarioId = userId, ObraDeArteId = obraId });
                await context.SaveChangesAsync();
            }

            var dto = new CreateCurtidaDto { ObraDeArteId = obraId };

            var response = await _client.PostAsJsonAsync("/api/Curtidas", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var status = await response.Content.ReadFromJsonAsync<CurtidaStatusDto>();

            status.Curtiu.Should().BeTrue();
            status.TotalCurtidas.Should().Be(1);
        }

        [Fact]
        public async Task Descurtir_QuandoJaCurtiu_DeveRemoverLikeEDecrementar()
        {
            var (userId, _) = await RegistrarEAutenticarAsync();
            var obraId = await CriarObraParaTesteAsync();

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.Curtidas.AddAsync(new Curtida { UsuarioId = userId, ObraDeArteId = obraId });
                await context.SaveChangesAsync();
            }

            var response = await _client.DeleteAsync($"/api/Curtidas/{obraId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var status = await response.Content.ReadFromJsonAsync<CurtidaStatusDto>();
            status.Curtiu.Should().BeFalse();
            status.TotalCurtidas.Should().Be(0);
        }

        [Fact]
        public async Task Descurtir_QuandoNaoHaviaCurtido_DeveRetornarSucessoSemAlterar()
        {
            await RegistrarEAutenticarAsync();
            var obraId = await CriarObraParaTesteAsync();

            var response = await _client.DeleteAsync($"/api/Curtidas/{obraId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var status = await response.Content.ReadFromJsonAsync<CurtidaStatusDto>();
            status.Curtiu.Should().BeFalse();
            status.TotalCurtidas.Should().Be(0);
        }

        [Fact]
        public async Task Descurtir_QuandoObraNaoExiste_DeveRetornarNotFound()
        {
            await RegistrarEAutenticarAsync();

            var response = await _client.DeleteAsync($"/api/Curtidas/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Descurtir_QuandoNaoAutenticado_DeveRetornarUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var response = await _client.DeleteAsync($"/api/Curtidas/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}