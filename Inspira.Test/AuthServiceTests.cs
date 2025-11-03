using Bogus;
using FluentAssertions;
using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using inspira_backend.Application.Services;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Enums;
using inspira_backend.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspira.Test
{
    public class AuthServiceTests
    {
        private readonly Mock<IUsuarioRepository> _mockUsuarioRepo;
        private readonly Mock<IJwtTokenGenerator> _mockJwtGenerator;
        private readonly IAuthService _service;

        private readonly Faker<Usuario> _usuarioFaker;
        private readonly Faker<RegisterRequestDto> _registerDtoFaker;
        private readonly Faker<LoginRequestDto> _loginDtoFaker;

        private readonly string _testPassword = "StrongPassword123!";
        private readonly string _testHash;

        public AuthServiceTests()
        {
            _mockUsuarioRepo = new Mock<IUsuarioRepository>();
            _mockJwtGenerator = new Mock<IJwtTokenGenerator>();
            _service = new AuthService(_mockUsuarioRepo.Object, _mockJwtGenerator.Object);

            _testHash = BCrypt.Net.BCrypt.HashPassword(_testPassword);

            _usuarioFaker = new Faker<Usuario>("pt_BR")
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.NomeUsuario, f => f.Internet.UserName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.SenhaHash, _testHash);

            _registerDtoFaker = new Faker<RegisterRequestDto>("pt_BR")
                .RuleFor(dto => dto.CompleteName, f => f.Name.FullName())
                .RuleFor(dto => dto.Username, f => f.Internet.UserName())
                .RuleFor(dto => dto.Email, f => f.Internet.Email())
                .RuleFor(dto => dto.Password, _testPassword)
                .RuleFor(dto => dto.Role, UserRole.Artista);

            _loginDtoFaker = new Faker<LoginRequestDto>("pt_BR")
                .RuleFor(dto => dto.Username, f => f.Internet.UserName())
                .RuleFor(dto => dto.Password, _testPassword);
        }

        [Fact]
        public async Task LoginAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            var request = _loginDtoFaker.Generate();
            _mockUsuarioRepo.Setup(repo => repo.GetByUsernameAsync(request.Username))
                            .ReturnsAsync((Usuario?)null);

            var result = await _service.LoginAsync(request);

            result.Should().BeNull();
            _mockJwtGenerator.Verify(gen => gen.GenerateToken(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIsInvalid_ShouldReturnNull()
        {
            var request = _loginDtoFaker.Generate();
            request.Password = "wrong-password";
            var usuario = _usuarioFaker.Generate();
            usuario.NomeUsuario = request.Username;

            _mockUsuarioRepo.Setup(repo => repo.GetByUsernameAsync(request.Username))
                            .ReturnsAsync(usuario);

            var result = await _service.LoginAsync(request);

            result.Should().BeNull();
            _mockJwtGenerator.Verify(gen => gen.GenerateToken(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WhenCredentialsAreValid_ShouldReturnLoginResponseDtoWithToken()
        {
            var request = _loginDtoFaker.Generate();
            var usuario = _usuarioFaker.Generate();
            usuario.NomeUsuario = request.Username;
            var fakeToken = "ey.fake.token";

            _mockUsuarioRepo.Setup(repo => repo.GetByUsernameAsync(request.Username))
                            .ReturnsAsync(usuario);

            _mockJwtGenerator.Setup(gen => gen.GenerateToken(usuario))
                             .Returns(fakeToken);

            var result = await _service.LoginAsync(request);

            result.Should().NotBeNull();
            result.Token.Should().Be(fakeToken);
            result.UserId.Should().Be(usuario.Id);
            result.Username.Should().Be(usuario.NomeUsuario);
        }

        [Fact]
        public async Task RegisterAsync_WhenUsernameAlreadyExists_ShouldReturnNull()
        {
            var request = _registerDtoFaker.Generate();
            _mockUsuarioRepo.Setup(repo => repo.GetByUsernameAsync(request.Username))
                            .ReturnsAsync(_usuarioFaker.Generate());

            var result = await _service.RegisterAsync(request);

            result.Should().BeNull();
            _mockUsuarioRepo.Verify(repo => repo.AddAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldReturnNull()
        {
            var request = _registerDtoFaker.Generate();
            _mockUsuarioRepo.Setup(repo => repo.GetByUsernameAsync(request.Username))
                            .ReturnsAsync((Usuario?)null);
            _mockUsuarioRepo.Setup(repo => repo.GetByEmailAsync(request.Email))
                            .ReturnsAsync(_usuarioFaker.Generate());

            var result = await _service.RegisterAsync(request);

            result.Should().BeNull();
            _mockUsuarioRepo.Verify(repo => repo.AddAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WhenRegistrationIsSuccessful_ShouldAddUserAndReturnLoginResponse()
        {
            var request = _registerDtoFaker.Generate();
            var fakeToken = "ey.fake.token.from.register";
            Usuario capturedUser = null;

            _mockUsuarioRepo.Setup(repo => repo.GetByUsernameAsync(request.Username))
                            .ReturnsAsync((Usuario?)null);
            _mockUsuarioRepo.Setup(repo => repo.GetByEmailAsync(request.Email))
                            .ReturnsAsync((Usuario?)null);

            _mockUsuarioRepo.Setup(repo => repo.AddAsync(It.IsAny<Usuario>()))
                            .Callback<Usuario>(u => capturedUser = u)
                            .Returns(Task.CompletedTask);

            _mockUsuarioRepo.Setup(repo => repo.GetByUsernameAsync(request.Username))
                            .ReturnsAsync(() => capturedUser);

            _mockJwtGenerator.Setup(gen => gen.GenerateToken(It.Is<Usuario>(u => u.NomeUsuario == request.Username)))
                             .Returns(fakeToken);

            var result = await _service.RegisterAsync(request);

            result.Should().NotBeNull();
            result.Token.Should().Be(fakeToken);
            result.Username.Should().Be(request.Username);

            _mockUsuarioRepo.Verify(repo => repo.AddAsync(It.Is<Usuario>(u =>
                u.NomeUsuario == request.Username && u.Email == request.Email
            )), Times.Once);

            capturedUser.Should().NotBeNull();
            BCrypt.Net.BCrypt.Verify(request.Password, capturedUser.SenhaHash).Should().BeTrue();
        }
    }
}
