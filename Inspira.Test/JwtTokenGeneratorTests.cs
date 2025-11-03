using Bogus;
using FluentAssertions;
using inspira_backend.Application.Interfaces;
using inspira_backend.Application.Services;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Inspira.Test
{
    public class JwtTokenGeneratorTests
    {
        private readonly IConfiguration _configuration;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly Faker<Usuario> _usuarioFaker;

        private readonly string _testSecretKey = "THIS_IS_A_STRONG_ENOUGH_TEST_SECRET_KEY_12345";
        private readonly string _testIssuer = "TestIssuer";
        private readonly string _testAudience = "TestAudience";
        private readonly string _testExpiryMinutes = "30";

        public JwtTokenGeneratorTests()
        {
            var inMemoryConfig = new Dictionary<string, string>
            {
                {"JwtSettings:Secret", _testSecretKey},
                {"JwtSettings:Issuer", _testIssuer},
                {"JwtSettings:Audience", _testAudience},
                {"JwtSettings:ExpiryMinutes", _testExpiryMinutes}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemoryConfig)
                .Build();

            _tokenGenerator = new JwtTokenGenerator(_configuration);

            _usuarioFaker = new Faker<Usuario>("pt_BR")
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.NomeUsuario, f => f.Internet.UserName())
                .RuleFor(u => u.TipoUsuario, f => f.PickRandom<UserRole>())
                .RuleFor(u => u.UrlFotoPerfil, f => f.Internet.Avatar());
        }

        [Fact]
        public void GenerateToken_WhenGivenValidUsuario_ShouldCreateTokenWithCorrectClaims()
        {
            var usuario = _usuarioFaker.Generate();

            var tokenString = _tokenGenerator.GenerateToken(usuario);

            tokenString.Should().NotBeNullOrEmpty();

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_testSecretKey)),
                ValidateIssuer = true,
                ValidIssuer = _testIssuer,
                ValidateAudience = true,
                ValidAudience = _testAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(tokenString, validationParameters, out SecurityToken validatedToken);

            principal.Should().NotBeNull();
            var jwtToken = validatedToken as JwtSecurityToken;
            jwtToken.Should().NotBeNull();

            principal.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.NameIdentifier && c.Value == usuario.Id.ToString());
            principal.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.Email && c.Value == usuario.Email);
            principal.Claims.Should().ContainSingle(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == usuario.NomeUsuario);
            principal.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.Role && c.Value == usuario.TipoUsuario.ToString());
            principal.Claims.Should().ContainSingle(c => c.Type == "urlPerfil" && c.Value == (usuario.UrlFotoPerfil ?? ""));

            var expectedExpiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_testExpiryMinutes));
            jwtToken.ValidTo.Should().BeCloseTo(expectedExpiry, precision: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void GenerateToken_WhenUrlFotoPerfilIsNull_ShouldCreateTokenWithEmptyUrlClaim()
        {
            var usuario = _usuarioFaker.Generate();
            usuario.UrlFotoPerfil = null;

            var tokenString = _tokenGenerator.GenerateToken(usuario);

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(tokenString);

            var urlClaim = token.Claims.FirstOrDefault(c => c.Type == "urlPerfil");
            urlClaim.Should().NotBeNull();
            urlClaim.Value.Should().BeEmpty();
        }
    }
}
