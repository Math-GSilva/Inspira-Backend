using Bogus;
using FluentAssertions;
using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using inspira_backend.Application.Services;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Globalization;

namespace Inspira.Test
{
    public class ObraDeArteServiceTests
    {
        private readonly Mock<IObraDeArteRepository> _mockObraRepo;
        private readonly Mock<IUsuarioRepository> _mockUsuarioRepo;
        private readonly Mock<ICategoriaRepository> _mockCategoriaRepo;
        private readonly Mock<IMediaUploadService> _mockMediaService;

        private readonly ObraDeArteService _service;

        private readonly Faker<Usuario> _usuarioFaker;
        private readonly Faker<Categoria> _categoriaFaker;
        private readonly Faker<ObraDeArte> _obraFaker;
        private readonly Faker<CreateObraDeArteDto> _createDtoFaker;
        private readonly Faker<UpdateObraDeArteDto> _updateDtoFaker;

        public ObraDeArteServiceTests()
        {
            _mockObraRepo = new Mock<IObraDeArteRepository>();
            _mockUsuarioRepo = new Mock<IUsuarioRepository>();
            _mockCategoriaRepo = new Mock<ICategoriaRepository>();
            _mockMediaService = new Mock<IMediaUploadService>();

            _service = new ObraDeArteService(
                _mockObraRepo.Object,
                _mockUsuarioRepo.Object,
                _mockCategoriaRepo.Object,
                _mockMediaService.Object
            );

            _usuarioFaker = new Faker<Usuario>("pt_BR")
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.NomeUsuario, f => f.Internet.UserName())
                .RuleFor(u => u.UrlFotoPerfil, f => f.Internet.Avatar());

            _categoriaFaker = new Faker<Categoria>("pt_BR")
                .RuleFor(c => c.Id, f => Guid.NewGuid())
                .RuleFor(c => c.Nome, f => f.Commerce.Categories(1)[0]);

            _obraFaker = new Faker<ObraDeArte>("pt_BR")
                .RuleFor(o => o.Id, f => Guid.NewGuid())
                .RuleFor(o => o.Titulo, f => f.Lorem.Sentence(3))
                .RuleFor(o => o.Descricao, f => f.Lorem.Paragraph())
                .RuleFor(o => o.DataPublicacao, f => f.Date.Past())
                .RuleFor(o => o.Usuario, () => _usuarioFaker.Generate())
                .RuleFor(o => o.Categoria, () => _categoriaFaker.Generate())
                .RuleFor(o => o.Curtidas, new List<Curtida>());

            _createDtoFaker = new Faker<CreateObraDeArteDto>()
                .RuleFor(dto => dto.Titulo, f => f.Lorem.Sentence(3))
                .RuleFor(dto => dto.Descricao, f => f.Lorem.Paragraph())
                .RuleFor(dto => dto.CategoriaId, f => Guid.NewGuid());

            _updateDtoFaker = new Faker<UpdateObraDeArteDto>()
                .RuleFor(dto => dto.Titulo, f => f.Lorem.Sentence(3))
                .RuleFor(dto => dto.Descricao, f => f.Lorem.Paragraph());
        }

        [Fact]
        public async Task GetByIdAsync_WhenObraExists_ShouldMapToDtoCorrectly()
        {
            var userIdLogado = Guid.NewGuid();
            var obra = _obraFaker.Generate();
            obra.Usuario = _usuarioFaker.Generate();
            obra.Categoria = _categoriaFaker.Generate();

            obra.Curtidas.Add(new Curtida { UsuarioId = Guid.NewGuid(), ObraDeArteId = obra.Id });
            obra.Curtidas.Add(new Curtida { UsuarioId = userIdLogado, ObraDeArteId = obra.Id });

            _mockObraRepo.Setup(repo => repo.GetByIdAsync(obra.Id, false)).ReturnsAsync(obra);

            var result = await _service.GetByIdAsync(obra.Id);
            _mockObraRepo.Setup(repo => repo.GetAllByUserAsync(userIdLogado)).ReturnsAsync(new List<ObraDeArte> { obra });
            var resultComUser = (await _service.GetAllByUserAsync(userIdLogado)).First();

            result.Should().NotBeNull();
            result.Titulo.Should().Be(obra.Titulo);
            result.AutorUsername.Should().Be(obra.Usuario.NomeUsuario);
            result.CategoriaNome.Should().Be(obra.Categoria.Nome);
            result.TotalCurtidas.Should().Be(2);

            result.CurtidaPeloUsuario.Should().BeFalse();
            resultComUser.CurtidaPeloUsuario.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_WhenAutorOrCategoriaNotFound_ShouldReturnNull()
        {
            var dto = _createDtoFaker.Generate();
            var userId = Guid.NewGuid();
            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((Usuario?)null);
            _mockCategoriaRepo.Setup(repo => repo.GetByIdAsync(dto.CategoriaId)).ReturnsAsync(_categoriaFaker.Generate());

            var result = await _service.CreateAsync(dto, userId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_WhenMediaIsInvalid_ShouldReturnNull()
        {
            var dto = _createDtoFaker.Generate();
            var userId = Guid.NewGuid();
            var autor = _usuarioFaker.Generate();
            var categoria = _categoriaFaker.Generate();

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("text/plain");
            dto.Midia = mockFile.Object;

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(autor);
            _mockCategoriaRepo.Setup(repo => repo.GetByIdAsync(dto.CategoriaId)).ReturnsAsync(categoria);

            var result = await _service.CreateAsync(dto, userId);

            result.Should().BeNull();
            _mockMediaService.Verify(s => s.UploadAsync(It.IsAny<IFormFile>()), Times.Never); // Upload nem deve ser tentado
        }

        [Fact]
        public async Task CreateAsync_WhenUploadFails_ShouldReturnNull()
        {
            var dto = _createDtoFaker.Generate();
            var userId = Guid.NewGuid();
            var autor = _usuarioFaker.Generate();
            var categoria = _categoriaFaker.Generate();

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("image/png");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns((Stream stream, CancellationToken token) => new MemoryStream().CopyToAsync(stream));
            dto.Midia = mockFile.Object;

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(autor);
            _mockCategoriaRepo.Setup(repo => repo.GetByIdAsync(dto.CategoriaId)).ReturnsAsync(categoria);

            _mockMediaService.Setup(s => s.UploadAsync(dto.Midia)).ReturnsAsync((string?)null);

            var result = await _service.CreateAsync(dto, userId);

            result.Should().BeNull();
            _mockMediaService.Verify(s => s.UploadAsync(dto.Midia), Times.Once);
            _mockObraRepo.Verify(r => r.AddAsync(It.IsAny<ObraDeArte>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenAllIsValid_ShouldAddAndReturnDto()
        {
            var dto = _createDtoFaker.Generate();
            var userId = Guid.NewGuid();
            var autor = _usuarioFaker.Generate();
            var categoria = _categoriaFaker.Generate();
            var mockUrl = "http://my-storage.com/image.png";

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("image/png");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns((Stream stream, CancellationToken token) => new MemoryStream().CopyToAsync(stream));
            dto.Midia = mockFile.Object;

            _mockUsuarioRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(autor);
            _mockCategoriaRepo.Setup(repo => repo.GetByIdAsync(dto.CategoriaId)).ReturnsAsync(categoria);
            _mockMediaService.Setup(s => s.UploadAsync(dto.Midia)).ReturnsAsync(mockUrl);

            var result = await _service.CreateAsync(dto, userId);

            result.Should().NotBeNull();
            result.Titulo.Should().Be(dto.Titulo);
            result.Url.Should().Be(mockUrl);
            result.TipoConteudoMidia.Should().Be("image/png");
            result.AutorUsername.Should().Be(autor.NomeUsuario);

            _mockObraRepo.Verify(repo => repo.AddAsync(
                It.Is<ObraDeArte>(obra =>
                    obra.Titulo == dto.Titulo &&
                    obra.UrlMidia == mockUrl &&
                    obra.UsuarioId == userId
                )
            ), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenUserIsNotAutor_ShouldThrowUnauthorizedAccessException()
        {
            var dto = _updateDtoFaker.Generate();
            var obraId = Guid.NewGuid();
            var userIdLogado = Guid.NewGuid();
            var autorId = Guid.NewGuid();

            var obraExistente = _obraFaker.Generate();
            obraExistente.UsuarioId = autorId;

            _mockObraRepo.Setup(repo => repo.GetByIdAsync(obraId, true)).ReturnsAsync(obraExistente);

            Func<Task> act = async () => await _service.UpdateAsync(obraId, dto, userIdLogado);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("Apenas o autor pode editar a obra.");

            _mockObraRepo.Verify(r => r.UpdateAsync(It.IsAny<ObraDeArte>()), Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_WhenCalledWithNoCursorAndHasMoreItems_ShouldReturnPaginatedResultWithNextCursor()
        {
            var userId = Guid.NewGuid();
            var pageSize = 2;
            var dataPublicacao = DateTime.UtcNow;

            var repoResults = new List<(ObraDeArte Obra, int IsLiked, double Score)>
            {
                (_obraFaker.Generate(), 1, 10.5),
                (_obraFaker.Generate(), 0, 8.5),
                (_obraFaker.Generate(), 1, 5.0)
            };
            repoResults[1].Obra.DataPublicacao = dataPublicacao;

            _mockObraRepo.Setup(r => r.GetAllAsync(userId, null, pageSize, null, null, null))
                         .ReturnsAsync(repoResults);

            var result = await _service.GetAllAsync(userId, null, pageSize, null);

            result.Items.Should().HaveCount(2);
            result.HasMoreItems.Should().BeTrue();
            result.NextCursor.Should().NotBeNullOrEmpty();

            var expectedCursor = $"0|{8.5.ToString(CultureInfo.InvariantCulture)}|{dataPublicacao:o}";
            result.NextCursor.Should().Be(expectedCursor);
        }

        [Fact]
        public async Task GetAllAsync_WhenCalledWithValidCursor_ShouldParseCursorAndCallRepository()
        {
            var userId = Guid.NewGuid();
            var pageSize = 5;
            var dataCursor = DateTime.UtcNow.AddHours(-1);
            var cursor = $"1|12.3|{dataCursor:o}";

            var repoResults = new List<(ObraDeArte Obra, int IsLiked, double Score)>();

            _mockObraRepo.Setup(r => r.GetAllAsync(userId, null, pageSize, 1, 12.3, It.Is<DateTime>(d => d.Kind == dataCursor.Kind && d == dataCursor)))
                         .ReturnsAsync(repoResults);

            var result = await _service.GetAllAsync(userId, null, pageSize, cursor);

            result.HasMoreItems.Should().BeFalse();
            result.Items.Should().BeEmpty();

            _mockObraRepo.Verify(r => r.GetAllAsync(
                userId,
                null,
                pageSize,
                1,
                12.3,
                It.Is<DateTime>(d => d.Kind == dataCursor.Kind && d == dataCursor)
            ), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenCalledWithInvalidCursor_ShouldCallRepositoryWithNulls()
        {
            var userId = Guid.NewGuid();
            var pageSize = 5;
            var invalidCursor = "isto-nao-e-um-cursor";

            var repoResults = new List<(ObraDeArte Obra, int IsLiked, double Score)>();
            _mockObraRepo.Setup(r => r.GetAllAsync(userId, null, pageSize, null, null, null))
                         .ReturnsAsync(repoResults);

            var result = await _service.GetAllAsync(userId, null, pageSize, invalidCursor);

            result.HasMoreItems.Should().BeFalse();

            _mockObraRepo.Verify(r => r.GetAllAsync(
                userId, null, pageSize, null, null, null
            ), Times.Once);
        }
    }
}
