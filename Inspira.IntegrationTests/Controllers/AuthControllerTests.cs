using FluentAssertions;
using inspira_backend.Application.DTOs;
using inspira_backend.Domain.Enums;
using System.Net;
using System.Net.Http.Json;

namespace Inspira.IntegrationTests.Controllers
{
    public class AuthControllerTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFixture _fixture;

        public AuthControllerTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task Register_QuandoDadosValidos_DeveRetornarSucessoEToken()
        {
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var request = new RegisterRequestDto
            {
                CompleteName = "Novo Usuario",
                Username = $"user_{uniqueId}",
                Email = $"user_{uniqueId}@teste.com",
                Password = "PasswordStrong123!",
                Role = UserRole.Artista
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.Username.Should().Be(request.Username);
        }

        [Fact]
        public async Task Register_QuandoUsernameJaExiste_DeveRetornarBadRequest()
        {
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var request = new RegisterRequestDto
            {
                CompleteName = "Usuario Duplicado",
                Username = $"dup_{uniqueId}",
                Email = $"dup_{uniqueId}@teste.com",
                Password = "PasswordStrong123!",
                Role = UserRole.Comum
            };

            var response1 = await _client.PostAsJsonAsync("/api/Auth/register", request);
            response1.EnsureSuccessStatusCode();

            var requestDuplicado = new RegisterRequestDto
            {
                CompleteName = "Outra Pessoa",
                Username = request.Username,
                Email = $"outro_{uniqueId}@teste.com",
                Password = "PasswordStrong123!",
                Role = UserRole.Comum
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/register", requestDuplicado);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.Should().Contain("Usuário ou e-mail já existente");
        }

        [Fact]
        public async Task Register_QuandoEmailJaExiste_DeveRetornarBadRequest()
        {
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var request = new RegisterRequestDto
            {
                CompleteName = "Usuario Original",
                Username = $"orig_{uniqueId}",
                Email = $"comum_{uniqueId}@teste.com",
                Password = "PasswordStrong123!",
                Role = UserRole.Comum
            };

            await _client.PostAsJsonAsync("/api/Auth/register", request);

            var requestDuplicado = new RegisterRequestDto
            {
                CompleteName = "Impostor",
                Username = $"impostor_{uniqueId}",
                Email = request.Email,
                Password = "PasswordStrong123!",
                Role = UserRole.Comum
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/register", requestDuplicado);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_QuandoCredenciaisValidas_DeveRetornarToken()
        {
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var password = "MySecretPassword123!";

            var registerRequest = new RegisterRequestDto
            {
                CompleteName = "Usuario Login",
                Username = $"login_{uniqueId}",
                Email = $"login_{uniqueId}@teste.com",
                Password = password,
                Role = UserRole.Artista
            };

            await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

            var loginRequest = new LoginRequestDto
            {
                Username = registerRequest.Username,
                Password = password
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            result.Token.Should().NotBeNullOrEmpty();
            result.UserId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Login_QuandoSenhaIncorreta_DeveRetornarUnauthorized()
        {
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var registerRequest = new RegisterRequestDto
            {
                CompleteName = "Usuario Senha Errada",
                Username = $"wrong_{uniqueId}",
                Email = $"wrong_{uniqueId}@teste.com",
                Password = "PasswordCorrect!",
                Role = UserRole.Comum
            };
            await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

            var loginRequest = new LoginRequestDto
            {
                Username = registerRequest.Username,
                Password = "PasswordWRONG!"
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var error = await response.Content.ReadAsStringAsync();
            error.Should().Contain("Credenciais inválidas");
        }

        [Fact]
        public async Task Login_QuandoUsuarioNaoExiste_DeveRetornarUnauthorized()
        {
            var loginRequest = new LoginRequestDto
            {
                Username = "usuario_fantasma_inexistente",
                Password = "123"
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}