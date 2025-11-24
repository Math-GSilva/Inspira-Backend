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
    public class ComentariosControllerTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFixture _fixture;

        public ComentariosControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.CreateClient();
        }
        private async Task<(Guid UserId, string Token)> AutenticarAsync(UserRole role = UserRole.Comum)
        {
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var registerDto = new RegisterRequestDto
            {
                CompleteName = $"User {uniqueId}",
                Username = $"user_{uniqueId}",
                Email = $"user_{uniqueId}@teste.com",
                Password = "PasswordStrong123!",
                Role = role
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);
            response.EnsureSuccessStatusCode();
            var loginResult = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResult.Token);

            return (loginResult.UserId, loginResult.Token);
        }

        private async Task<Guid> CriarObraDeArteNoBancoAsync(Guid autorId)
        {
            using var scope = _fixture.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();

            var categoria = new Categoria { Nome = $"Cat {Guid.NewGuid()}" };
            await context.Categorias.AddAsync(categoria);
            await context.SaveChangesAsync();

            var obra = new ObraDeArte
            {
                Titulo = "Obra Comentada",
                Descricao = "Teste",
                UsuarioId = autorId,
                CategoriaId = categoria.Id,
                DataPublicacao = DateTime.UtcNow,
                UrlMidia = "http://fake",
                TipoConteudoMidia = "image/png"
            };

            await context.ObrasDeArte.AddAsync(obra);
            await context.SaveChangesAsync();

            return obra.Id;
        }

        [Fact]
        public async Task GetComentarios_QuandoExistem_DeveRetornarLista()
        {
            var (userId, _) = await AutenticarAsync();
            var obraId = await CriarObraDeArteNoBancoAsync(userId);

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.Comentarios.AddAsync(new Comentario
                {
                    Conteudo = "Adorei!",
                    UsuarioId = userId,
                    ObraDeArteId = obraId,
                    DataComentario = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

            var response = await _client.GetAsync($"/api/Comentarios?obraDeArteId={obraId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var comentarios = await response.Content.ReadFromJsonAsync<List<ComentarioResponseDto>>();
            comentarios.Should().ContainSingle(c => c.Conteudo == "Adorei!");
        }

        [Fact]
        public async Task GetComentarios_QuandoNaoExistem_DeveRetornarListaVazia()
        {
            var (userId, _) = await AutenticarAsync();
            var obraId = await CriarObraDeArteNoBancoAsync(userId);

            var response = await _client.GetAsync($"/api/Comentarios?obraDeArteId={obraId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var comentarios = await response.Content.ReadFromJsonAsync<List<ComentarioResponseDto>>();
            comentarios.Should().BeEmpty();
        }

        [Fact]
        public async Task Criar_QuandoAutenticado_DeveRetornarSucesso()
        {
            var (userId, _) = await AutenticarAsync();
            var obraId = await CriarObraDeArteNoBancoAsync(userId);

            var dto = new CreateComentarioDto
            {
                ObraDeArteId = obraId,
                Conteudo = "Comentário de Teste"
            };

            var response = await _client.PostAsJsonAsync("/api/Comentarios", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var comentario = await response.Content.ReadFromJsonAsync<ComentarioResponseDto>();
            comentario.Conteudo.Should().Be("Comentário de Teste");
            comentario.AutorId.Should().Be(userId);
        }

        [Fact]
        public async Task Criar_QuandoObraNaoExiste_DeveRetornarNotFound()
        {
            await AutenticarAsync();
            var dto = new CreateComentarioDto
            {
                ObraDeArteId = Guid.NewGuid(),
                Conteudo = "Fantasma"
            };

            var response = await _client.PostAsJsonAsync("/api/Comentarios", dto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Criar_QuandoNaoAutenticado_DeveRetornarUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;
            var dto = new CreateComentarioDto { ObraDeArteId = Guid.NewGuid(), Conteudo = "X" };

            var response = await _client.PostAsJsonAsync("/api/Comentarios", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Delete_QuandoAutor_DeveDeletar()
        {
            var (userId, _) = await AutenticarAsync();
            var obraId = await CriarObraDeArteNoBancoAsync(userId);
            var comentarioId = Guid.NewGuid();

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.Comentarios.AddAsync(new Comentario
                {
                    Id = comentarioId,
                    Conteudo = "Meu Comentário",
                    UsuarioId = userId,
                    ObraDeArteId = obraId
                });
                await context.SaveChangesAsync();
            }

            var response = await _client.DeleteAsync($"/api/Comentarios/{comentarioId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                var exists = await context.Comentarios.FindAsync(comentarioId);
                exists.Should().BeNull();
            }
        }

        [Fact]
        public async Task Delete_QuandoAdmin_DeveDeletarQualquerComentario()
        {
            var (userIdComum, _) = await AutenticarAsync(UserRole.Comum);
            var obraId = await CriarObraDeArteNoBancoAsync(userIdComum);
            var comentarioId = Guid.NewGuid();

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.Comentarios.AddAsync(new Comentario
                {
                    Id = comentarioId,
                    Conteudo = "Comentário ofensivo",
                    UsuarioId = userIdComum,
                    ObraDeArteId = obraId
                });
                await context.SaveChangesAsync();
            }

            await AutenticarAsync(UserRole.Administrador);

            var response = await _client.DeleteAsync($"/api/Comentarios/{comentarioId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_QuandoOutroUsuario_DeveRetornarForbidden()
        {
            var (autorId, _) = await AutenticarAsync();
            var obraId = await CriarObraDeArteNoBancoAsync(autorId);
            var comentarioId = Guid.NewGuid();

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.Comentarios.AddAsync(new Comentario
                {
                    Id = comentarioId,
                    Conteudo = "Comentário alheio",
                    UsuarioId = autorId,
                    ObraDeArteId = obraId
                });
                await context.SaveChangesAsync();
            }

            await AutenticarAsync(UserRole.Comum);

            var response = await _client.DeleteAsync($"/api/Comentarios/{comentarioId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_QuandoNaoExiste_DeveRetornarNotFound()
        {
            await AutenticarAsync();
            var response = await _client.DeleteAsync($"/api/Comentarios/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}