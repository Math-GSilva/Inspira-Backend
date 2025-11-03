using Bogus;
using FluentAssertions;
using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using inspira_backend.Application.Services;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Inspira.Test
{
    public class UsuarioServiceTests
    {
        private readonly Mock<IUsuarioRepository> _mockUsuarioRepo;
        private readonly Mock<ISeguidorRepository> _mockSeguidorRepo;
        private readonly Mock<IMediaUploadService> _mockMediaService;

        private readonly UsuarioService _service;

        private readonly Faker<Usuario> _usuarioFaker;
        private readonly Faker<UpdateUsuarioDto> _updateDtoFaker;

        public UsuarioServiceTests()
        {
            _mockUsuarioRepo = new Mock<IUsuarioRepository>();
            _mockSeguidorRepo = new Mock<ISeguidorRepository>();
            _mockMediaService = new Mock<IMediaUploadService>();

            _service = new UsuarioService(
                _mockUsuarioRepo.Object,
                _mockSeguidorRepo.Object,
                _mockMediaService.Object
            );

            _usuarioFaker = new Faker<Usuario>("pt_BR")
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.NomeUsuario, f => f.Internet.UserName())
                .RuleFor(u => u.NomeCompleto, f => f.Name.FullName())
                .RuleFor(u => u.Bio, f => f.Lorem.Sentence())
                .RuleFor(u => u.UrlFotoPerfil, f => f.Internet.Avatar())
                .RuleFor(u => u.Seguidores, new List<Seguidor>())
                .RuleFor(u => u.Seguindo, new List<Seguidor>());

            _updateDtoFaker = new Faker<UpdateUsuarioDto>("pt_BR")
                .RuleFor(u => u.Bio, f => f.Lorem.Sentence())
                .RuleFor(u => u.UrlInstagram, f => f.Internet.Url())
                .RuleFor(u => u.UrlLinkedin, f => f.Internet.Url())
                .RuleFor(u => u.UrlPortifolio, f => f.Internet.Url());
        }


        [Fact]
        public async Task GetProfileByUsernameAsync_WhenUserExists_ShouldReturnUserProfileDto()
        {
            var userIdLogado = Guid.NewGuid();
            var usernamePesquisado = "testuser";

            var usuarioFalso = _usuarioFaker.Generate();
            usuarioFalso.NomeUsuario = usernamePesquisado;

            _mockUsuarioRepo.Setup(repo => repo.GetByUsernameAsync(usernamePesquisado))
                            .ReturnsAsync(usuarioFalso);

            var result = await _service.GetProfileByUsernameAsync(usernamePesquisado, userIdLogado);

            result.Should().NotBeNull();
            result.Username.Should().Be(usernamePesquisado);
            result.NomeCompleto.Should().Be(usuarioFalso.NomeCompleto);
            result.SeguidoPeloUsuarioAtual.Should().BeFalse();
        }

        [Fact]
        public async Task GetProfileByUsernameAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            var usernamePesquisado = "user_nao_existe";

            _mockUsuarioRepo.Setup(repo => repo.GetByUsernameAsync(usernamePesquisado))
                            .ReturnsAsync((Usuario?)null);

            var result = await _service.GetProfileByUsernameAsync(usernamePesquisado, Guid.NewGuid());

            result.Should().BeNull();
        }

        [Fact]
        public async Task FollowUserAsync_WhenUserTriesToFollowSelf_ShouldReturnFalse()
        {
            var userId = Guid.NewGuid();

            var result = await _service.FollowUserAsync(userId, userId);

            result.Should().BeFalse();
            _mockSeguidorRepo.Verify(repo => repo.AddAsync(It.IsAny<Seguidor>()), Times.Never);
        }

        [Fact]
        public async Task FollowUserAsync_WhenUserToFollowDoesNotExist_ShouldReturnFalse()
        {
            var seguidorId = Guid.NewGuid();
            var seguidoId = Guid.NewGuid();

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(seguidoId))
                            .ReturnsAsync((Usuario?)null);

            var result = await _service.FollowUserAsync(seguidorId, seguidoId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task FollowUserAsync_WhenNotAlreadyFollowing_ShouldAddFollowAndReturnTrue()
        {
            var seguidorId = Guid.NewGuid();
            var seguidoId = Guid.NewGuid();
            var usuarioASeguir = _usuarioFaker.Generate();

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(seguidoId))
                            .ReturnsAsync(usuarioASeguir);

            _mockSeguidorRepo.Setup(repo => repo.GetByFollowerAndFollowedAsync(seguidorId, seguidoId))
                             .ReturnsAsync((Seguidor?)null);

            var result = await _service.FollowUserAsync(seguidorId, seguidoId);

            result.Should().BeTrue();
            _mockSeguidorRepo.Verify(repo => repo.AddAsync(
                It.Is<Seguidor>(s => s.SeguidorId == seguidorId && s.SeguidoId == seguidoId)
            ), Times.Once);
        }

        [Fact]
        public async Task FollowUserAsync_WhenAlreadyFollowing_ShouldReturnTrueAndNotAdd()
        {
            var seguidorId = Guid.NewGuid();
            var seguidoId = Guid.NewGuid();
            var usuarioASeguir = _usuarioFaker.Generate();

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(seguidoId))
                            .ReturnsAsync(usuarioASeguir);

            _mockSeguidorRepo.Setup(repo => repo.GetByFollowerAndFollowedAsync(seguidorId, seguidoId))
                             .ReturnsAsync(new Seguidor { SeguidorId = seguidorId, SeguidoId = seguidoId });

            var result = await _service.FollowUserAsync(seguidorId, seguidoId);

            result.Should().BeTrue();
            _mockSeguidorRepo.Verify(repo => repo.AddAsync(It.IsAny<Seguidor>()), Times.Never);
        }

        [Fact]
        public async Task UnfollowUserAsync_WhenFollowExists_ShouldDeleteFollowAndReturnTrue()
        {
            var seguidorId = Guid.NewGuid();
            var seguidoId = Guid.NewGuid();
            var relacaoExistente = new Seguidor { SeguidorId = seguidorId, SeguidoId = seguidoId };

            _mockSeguidorRepo.Setup(repo => repo.GetByFollowerAndFollowedAsync(seguidorId, seguidoId))
                             .ReturnsAsync(relacaoExistente);

            var result = await _service.UnfollowUserAsync(seguidorId, seguidoId);

            result.Should().BeTrue();
            _mockSeguidorRepo.Verify(repo => repo.DeleteAsync(relacaoExistente), Times.Once);
        }

        [Fact]
        public async Task UnfollowUserAsync_WhenFollowDoesNotExist_ShouldReturnFalse()
        {
            var seguidorId = Guid.NewGuid();
            var seguidoId = Guid.NewGuid();

            _mockSeguidorRepo.Setup(repo => repo.GetByFollowerAndFollowedAsync(seguidorId, seguidoId))
                             .ReturnsAsync((Seguidor?)null);

            var result = await _service.UnfollowUserAsync(seguidorId, seguidoId);

            result.Should().BeFalse();
            _mockSeguidorRepo.Verify(repo => repo.DeleteAsync(It.IsAny<Seguidor>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenUserExistsAndPhotoIsUploaded_ShouldUpdateUrl()
        {
            var userId = Guid.NewGuid();
            var dto = _updateDtoFaker.Generate();
            var usuarioExistente = _usuarioFaker.Generate();
            usuarioExistente.Id = userId;

            var novaUrlFoto = "http://azure.com/nova-foto.jpg";

            var mockFoto = new Mock<IFormFile>();
            mockFoto.Setup(f => f.Length).Returns(1024);
            dto.FotoPerfil = mockFoto.Object;

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(usuarioExistente);
            _mockMediaService.Setup(s => s.UploadAsync(dto.FotoPerfil)).ReturnsAsync(novaUrlFoto);

            _mockUsuarioRepo.Setup(repo => repo.GetByUsernameAsync(usuarioExistente.NomeUsuario))
                            .ReturnsAsync(usuarioExistente);

            var result = await _service.UpdateProfileAsync(userId, dto);

            result.Should().NotBeNull();
            _mockMediaService.Verify(s => s.UploadAsync(dto.FotoPerfil), Times.Once);

            _mockUsuarioRepo.Verify(repo => repo.UpdateAsync(
                It.Is<Usuario>(u =>
                    u.Id == userId &&
                    u.UrlFotoPerfil == novaUrlFoto &&
                    u.Bio == dto.Bio
                )
            ), Times.Once);
        }
    }
}
