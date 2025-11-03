using Bogus;
using FluentAssertions;
using inspira_backend.Application.Services;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspira.Test
{
    public class SeguidorServiceTests
    {
        private readonly Mock<IUsuarioRepository> _mockUsuarioRepo;

        private readonly SeguidorService _service;

        private readonly Faker<Usuario> _usuarioFaker;
        private readonly Faker<Seguidor> _seguidorFaker;

        public SeguidorServiceTests()
        {
            _mockUsuarioRepo = new Mock<IUsuarioRepository>();

            _service = new SeguidorService(_mockUsuarioRepo.Object);

            _usuarioFaker = new Faker<Usuario>("pt_BR")
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.NomeUsuario, f => f.Internet.UserName())
                .RuleFor(u => u.NomeCompleto, f => f.Name.FullName())
                .RuleFor(u => u.UrlFotoPerfil, f => f.Internet.Avatar())
                .RuleFor(u => u.Seguidores, new List<Seguidor>())
                .RuleFor(u => u.Seguindo, new List<Seguidor>());

            _seguidorFaker = new Faker<Seguidor>()
                .RuleFor(s => s.SeguidorId, f => Guid.NewGuid())
                .RuleFor(s => s.SeguidoId, f => Guid.NewGuid())
                .RuleFor(s => s.SeguidorUsuario, () => _usuarioFaker.Generate())
                .RuleFor(s => s.SeguidoUsuario, () => _usuarioFaker.Generate());
        }

        [Fact]
        public async Task GetSeguidoresAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            var usuarioId = Guid.NewGuid();

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(usuarioId))
                            .ReturnsAsync((Usuario?)null);

            var result = await _service.GetSeguidoresAsync(usuarioId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSeguidoresAsync_WhenUserExistsButHasNoFollowers_ShouldReturnEmptyList()
        {
            var usuarioId = Guid.NewGuid();
            var usuario = _usuarioFaker.Generate();
            usuario.Id = usuarioId;
            usuario.Seguidores = new List<Seguidor>();

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(usuarioId))
                            .ReturnsAsync(usuario);

            var result = await _service.GetSeguidoresAsync(usuarioId);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSeguidoresAsync_WhenUserExistsWithFollowers_ShouldReturnMappedDtoList()
        {
            var usuarioId = Guid.NewGuid();
            var usuario = _usuarioFaker.Generate();
            usuario.Id = usuarioId;

            usuario.Seguidores = _seguidorFaker.Generate(3);

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(usuarioId))
                            .ReturnsAsync(usuario);

            var primeiroSeguidor = usuario.Seguidores.First();

            var result = await _service.GetSeguidoresAsync(usuarioId);

            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            var primeiroDto = result.First();
            primeiroDto.UsuarioId.Should().Be(primeiroSeguidor.SeguidorUsuario.Id);
            primeiroDto.Username.Should().Be(primeiroSeguidor.SeguidorUsuario.NomeUsuario);
            primeiroDto.NomeCompleto.Should().Be(primeiroSeguidor.SeguidorUsuario.NomeCompleto);
        }

        [Fact]
        public async Task GetSeguindoAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            var usuarioId = Guid.NewGuid();

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(usuarioId))
                            .ReturnsAsync((Usuario?)null);

            var result = await _service.GetSeguindoAsync(usuarioId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSeguindoAsync_WhenUserExistsButFollowingNoOne_ShouldReturnEmptyList()
        {
            var usuarioId = Guid.NewGuid();
            var usuario = _usuarioFaker.Generate();
            usuario.Id = usuarioId;
            usuario.Seguindo = new List<Seguidor>();

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(usuarioId))
                            .ReturnsAsync(usuario);

            var result = await _service.GetSeguindoAsync(usuarioId);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSeguindoAsync_WhenUserExistsAndIsFollowing_ShouldReturnMappedDtoList()
        {
            var usuarioId = Guid.NewGuid();
            var usuario = _usuarioFaker.Generate();
            usuario.Id = usuarioId;

            usuario.Seguindo = _seguidorFaker.Generate(2);

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(usuarioId))
                            .ReturnsAsync(usuario);

            var primeiroSeguido = usuario.Seguindo.First();

            var result = await _service.GetSeguindoAsync(usuarioId);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var primeiroDto = result.First();
            primeiroDto.UsuarioId.Should().Be(primeiroSeguido.SeguidoUsuario.Id);
            primeiroDto.Username.Should().Be(primeiroSeguido.SeguidoUsuario.NomeUsuario);
            primeiroDto.NomeCompleto.Should().Be(primeiroSeguido.SeguidoUsuario.NomeCompleto);
        }
    }
}
