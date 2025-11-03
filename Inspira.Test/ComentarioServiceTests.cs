using Bogus;
using FluentAssertions;
using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
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
    public class ComentarioServiceTests
    {
        private readonly Mock<IComentarioRepository> _mockComentarioRepo;
        private readonly Mock<IObraDeArteRepository> _mockObraRepo;
        private readonly Mock<IUsuarioRepository> _mockUsuarioRepo;
        private readonly IComentarioService _service;

        private readonly Faker<Usuario> _usuarioFaker;
        private readonly Faker<ObraDeArte> _obraFaker;
        private readonly Faker<Comentario> _comentarioFaker;
        private readonly Faker<CreateComentarioDto> _createDtoFaker;

        public ComentarioServiceTests()
        {
            _mockComentarioRepo = new Mock<IComentarioRepository>();
            _mockObraRepo = new Mock<IObraDeArteRepository>();
            _mockUsuarioRepo = new Mock<IUsuarioRepository>();

            _service = new ComentarioService(
                _mockComentarioRepo.Object,
                _mockObraRepo.Object,
                _mockUsuarioRepo.Object
            );

            _usuarioFaker = new Faker<Usuario>("pt_BR")
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.NomeUsuario, f => f.Internet.UserName())
                .RuleFor(u => u.UrlFotoPerfil, f => f.Internet.Avatar());

            _obraFaker = new Faker<ObraDeArte>("pt_BR")
                .RuleFor(o => o.Id, f => Guid.NewGuid());

            _comentarioFaker = new Faker<Comentario>("pt_BR")
                .RuleFor(c => c.Id, f => Guid.NewGuid())
                .RuleFor(c => c.Conteudo, f => f.Lorem.Sentence())
                .RuleFor(c => c.DataComentario, f => f.Date.Past())
                .RuleFor(c => c.UsuarioId, f => Guid.NewGuid())
                .RuleFor(c => c.Usuario, () => _usuarioFaker.Generate())
                .RuleFor(c => c.Respostas, new List<Comentario>());

            _createDtoFaker = new Faker<CreateComentarioDto>("pt_BR")
                .RuleFor(dto => dto.Conteudo, f => f.Lorem.Sentence())
                .RuleFor(dto => dto.ObraDeArteId, f => Guid.NewGuid());
        }

        [Fact]
        public async Task CriarComentarioAsync_WhenObraDeArteDoesNotExist_ShouldReturnNull()
        {
            var dto = _createDtoFaker.Generate();
            var userId = Guid.NewGuid();

            _mockObraRepo.Setup(repo => repo.GetByIdAsync(dto.ObraDeArteId, false))
                         .ReturnsAsync((ObraDeArte?)null);

            var result = await _service.CriarComentarioAsync(dto, userId);

            result.Should().BeNull();
            _mockComentarioRepo.Verify(repo => repo.AddAsync(It.IsAny<Comentario>()), Times.Never);
        }

        [Fact]
        public async Task CriarComentarioAsync_WhenValid_ShouldAddAndReturnMappedDto()
        {
            var dto = _createDtoFaker.Generate();
            var userId = Guid.NewGuid();
            var obra = _obraFaker.Generate();
            var autor = _usuarioFaker.Generate();
            autor.Id = userId;

            _mockObraRepo.Setup(repo => repo.GetByIdAsync(dto.ObraDeArteId, false))
                         .ReturnsAsync(obra);

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(userId))
                            .ReturnsAsync(autor);

            var result = await _service.CriarComentarioAsync(dto, userId);

            result.Should().NotBeNull();
            result.Conteudo.Should().Be(dto.Conteudo);
            result.AutorId.Should().Be(userId);
            result.AutorUsername.Should().Be(autor.NomeUsuario);
            result.UrlFotoPerfil.Should().Be(autor.UrlFotoPerfil);

            _mockComentarioRepo.Verify(repo => repo.AddAsync(
                It.Is<Comentario>(c =>
                    c.Conteudo == dto.Conteudo &&
                    c.UsuarioId == userId &&
                    c.ObraDeArteId == dto.ObraDeArteId
                )
            ), Times.Once);
        }

        [Fact]
        public async Task GetComentariosByObraDeArteIdAsync_WhenNoCommentsExist_ShouldReturnEmptyList()
        {
            var obraId = Guid.NewGuid();
            var emptyList = new List<Comentario>();

            _mockComentarioRepo.Setup(repo => repo.GetByObraDeArteIdAsync(obraId))
                               .ReturnsAsync(emptyList);

            var result = await _service.GetComentariosByObraDeArteIdAsync(obraId);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetComentariosByObraDeArteIdAsync_WhenCommentsExist_ShouldReturnMappedDtoList()
        {
            var obraId = Guid.NewGuid();
            var commentsList = _comentarioFaker.Generate(3);

            _mockComentarioRepo.Setup(repo => repo.GetByObraDeArteIdAsync(obraId))
                               .ReturnsAsync(commentsList);

            var result = await _service.GetComentariosByObraDeArteIdAsync(obraId);

            result.Should().HaveCount(3);
            result.First().Id.Should().Be(commentsList.First().Id);
            result.First().AutorUsername.Should().Be(commentsList.First().Usuario.NomeUsuario);
        }

        [Fact]
        public async Task DeleteComentarioAsync_WhenCommentDoesNotExist_ShouldReturnFalse()
        {
            var comentarioId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var roles = new List<string> { "Usuario" };

            _mockComentarioRepo.Setup(repo => repo.GetByIdAsync(comentarioId))
                               .ReturnsAsync((Comentario?)null);

            var result = await _service.DeleteComentarioAsync(comentarioId, userId, roles);

            result.Should().BeFalse();
            _mockComentarioRepo.Verify(repo => repo.DeleteAsync(It.IsAny<Comentario>()), Times.Never);
        }

        [Fact]
        public async Task DeleteComentarioAsync_WhenUserIsAuthor_ShouldDeleteAndReturnTrue()
        {
            var comentarioId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var roles = new List<string> { "Usuario" };
            var comentario = _comentarioFaker.Generate();
            comentario.Id = comentarioId;
            comentario.UsuarioId = userId;

            _mockComentarioRepo.Setup(repo => repo.GetByIdAsync(comentarioId))
                               .ReturnsAsync(comentario);

            var result = await _service.DeleteComentarioAsync(comentarioId, userId, roles);

            result.Should().BeTrue();
            _mockComentarioRepo.Verify(repo => repo.DeleteAsync(comentario), Times.Once);
        }

        [Fact]
        public async Task DeleteComentarioAsync_WhenUserIsAdmin_ShouldDeleteAndReturnTrue()
        {
            var comentarioId = Guid.NewGuid();
            var autorId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var roles = new List<string> { "Administrador" };
            var comentario = _comentarioFaker.Generate();
            comentario.Id = comentarioId;
            comentario.UsuarioId = autorId;

            _mockComentarioRepo.Setup(repo => repo.GetByIdAsync(comentarioId))
                               .ReturnsAsync(comentario);

            var result = await _service.DeleteComentarioAsync(comentarioId, adminId, roles);

            result.Should().BeTrue();
            _mockComentarioRepo.Verify(repo => repo.DeleteAsync(comentario), Times.Once);
        }

        [Fact]
        public async Task DeleteComentarioAsync_WhenUserIsUnauthorized_ShouldThrowUnauthorizedAccessException()
        {
            var comentarioId = Guid.NewGuid();
            var autorId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var roles = new List<string> { "Usuario" };
            var comentario = _comentarioFaker.Generate();
            comentario.Id = comentarioId;
            comentario.UsuarioId = autorId;

            _mockComentarioRepo.Setup(repo => repo.GetByIdAsync(comentarioId))
                               .ReturnsAsync(comentario);

            Func<Task> act = async () => await _service.DeleteComentarioAsync(comentarioId, otherUserId, roles);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("Apenas o autor do comentário ou um administrador podem apagar.");

            _mockComentarioRepo.Verify(repo => repo.DeleteAsync(It.IsAny<Comentario>()), Times.Never);
        }
    }
}
