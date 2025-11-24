using FluentAssertions;
using inspira_backend.Application.DTOs;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Enums;
using inspira_backend.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Inspira.IntegrationTests.Controllers
{
    public class ObrasDeArteControllerTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFixture _fixture;

        public ObrasDeArteControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.CreateClient();
        }
        private async Task<(Guid UserId, string Token)> AutenticarAsync(UserRole role = UserRole.Artista)
        {
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var registerDto = new RegisterRequestDto
            {
                CompleteName = $"Artista {uniqueId}",
                Username = $"art_{uniqueId}",
                Email = $"art_{uniqueId}@teste.com",
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
        private async Task<Guid> CriarCategoriaNoBancoAsync()
        {
            using var scope = _fixture.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();

            var categoria = new Categoria { Nome = $"Cat {Guid.NewGuid()}", Descricao = "Teste" };
            await context.Categorias.AddAsync(categoria);
            await context.SaveChangesAsync();

            return categoria.Id;
        }

        [Fact]
        public async Task Create_QuandoDadosValidos_DeveCriarObraComUploadFake()
        {
            await AutenticarAsync(UserRole.Artista);
            var categoriaId = await CriarCategoriaNoBancoAsync();

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent("Mona Lisa Teste"), "Titulo");
            form.Add(new StringContent("Uma descrição de teste"), "Descricao");
            form.Add(new StringContent(categoriaId.ToString()), "CategoriaId");

            var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            form.Add(fileContent, "Midia", "monalisa.jpg");

            var response = await _client.PostAsync("/api/ObrasDeArte", form);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var resultado = await response.Content.ReadFromJsonAsync<ObraDeArteResponseDto>();
            resultado.Titulo.Should().Be("Mona Lisa Teste");
            resultado.Url.Should().Contain("fake-cloudinary");
        }

        [Fact]
        public async Task Create_QuandoArquivoInvalido_DeveRetornarBadRequest()
        {
            await AutenticarAsync(UserRole.Artista);
            var categoriaId = await CriarCategoriaNoBancoAsync();

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent("Texto Inválido"), "Titulo");
            form.Add(new StringContent("Desc"), "Descricao");
            form.Add(new StringContent(categoriaId.ToString()), "CategoriaId");

            var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 });
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
            form.Add(fileContent, "Midia", "documento.pdf");

            var response = await _client.PostAsync("/api/ObrasDeArte", form);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_QuandoRoleComum_DeveRetornarForbidden()
        {
            await AutenticarAsync(UserRole.Comum);
            var categoriaId = await CriarCategoriaNoBancoAsync();

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent("Titulo"), "Titulo");

            var response = await _client.PostAsync("/api/ObrasDeArte", form);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAll_DeveRetornarPaginaComResultados()
        {
            var (userId, _) = await AutenticarAsync(UserRole.Artista);
            var categoriaId = await CriarCategoriaNoBancoAsync();

            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
                await context.ObrasDeArte.AddAsync(new ObraDeArte
                {
                    Titulo = "Obra 1",
                    Descricao = "D",
                    UsuarioId = userId,
                    CategoriaId = categoriaId,
                    DataPublicacao = DateTime.UtcNow,
                    UrlMidia = "http://url.fake",
                    TipoConteudoMidia = "image/png"
                });
                await context.SaveChangesAsync();
            }

            var response = await _client.GetAsync("/api/ObrasDeArte");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var paginacao = await response.Content.ReadFromJsonAsync<PaginatedResponseDto<ObraDeArteResponseDto>>();
            paginacao.Items.Should().NotBeEmpty();
            paginacao.Items.First().Titulo.Should().Be("Obra 1");
        }

        [Fact]
        public async Task Update_QuandoDono_DeveAtualizar()
        {
            var (userId, _) = await AutenticarAsync(UserRole.Artista);
            var obraId = await CriarObraNoBanco(userId); 

            var updateDto = new UpdateObraDeArteDto { Titulo = "Novo Título", Descricao = "Nova Desc" };

            var response = await _client.PutAsJsonAsync($"/api/ObrasDeArte/{obraId}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var resultado = await response.Content.ReadFromJsonAsync<ObraDeArteResponseDto>();
            resultado.Titulo.Should().Be("Novo Título");
        }

        [Fact]
        public async Task Update_QuandoOutroUsuario_DeveRetornarForbidden()
        {
            var (donoId, _) = await AutenticarAsync(UserRole.Artista);
            var obraId = await CriarObraNoBanco(donoId);

            await AutenticarAsync(UserRole.Artista);

            var updateDto = new UpdateObraDeArteDto { Titulo = "Hackeado", Descricao = "X" };

            var response = await _client.PutAsJsonAsync($"/api/ObrasDeArte/{obraId}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_QuandoAdmin_DeveDeletarQualquerObra()
        {
            var (donoId, _) = await AutenticarAsync(UserRole.Artista);
            var obraId = await CriarObraNoBanco(donoId);

            await AutenticarAsync(UserRole.Administrador);

            var response = await _client.DeleteAsync($"/api/ObrasDeArte/{obraId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using var scope = _fixture.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();
            var obra = await context.ObrasDeArte.FindAsync(obraId);
            obra.Should().BeNull();
        }

        private async Task<Guid> CriarObraNoBanco(Guid userId)
        {
            using var scope = _fixture.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InspiraDbContext>();

            var categoriaId = (await context.Categorias.FirstOrDefaultAsync())?.Id
                              ?? await CriarCategoriaNoBancoAsync();

            var obra = new ObraDeArte
            {
                Titulo = "Obra Base",
                Descricao = "Desc",
                UsuarioId = userId,
                CategoriaId = categoriaId,
                DataPublicacao = DateTime.UtcNow,
                UrlMidia = "http://fake",
                TipoConteudoMidia = "image/png"
            };

            await context.ObrasDeArte.AddAsync(obra);
            await context.SaveChangesAsync();
            return obra.Id;
        }
    }
}