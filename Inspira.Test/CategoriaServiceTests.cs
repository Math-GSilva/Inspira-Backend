using Bogus;
using FluentAssertions;
using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using inspira_backend.Application.Services;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using Moq;

namespace Inspira.Test
{
    public class CategoriaServiceTests
    {
        private readonly Mock<ICategoriaRepository> _mockCategoriaRepo;
        private readonly ICategoriaService _service;

        private readonly Faker<Categoria> _categoriaFaker;
        private readonly Faker<CreateCategoriaDto> _createDtoFaker;
        private readonly Faker<UpdateCategoriaDto> _updateDtoFaker;

        public CategoriaServiceTests()
        {
            _mockCategoriaRepo = new Mock<ICategoriaRepository>();
            _service = new CategoriaService(_mockCategoriaRepo.Object);

            _categoriaFaker = new Faker<Categoria>("pt_BR")
                .RuleFor(c => c.Id, f => Guid.NewGuid())
                .RuleFor(c => c.Nome, f => f.Commerce.Categories(1)[0])
                .RuleFor(c => c.Descricao, f => f.Lorem.Sentence());

            _createDtoFaker = new Faker<CreateCategoriaDto>("pt_BR")
                .RuleFor(dto => dto.Nome, f => f.Commerce.Categories(1)[0])
                .RuleFor(dto => dto.Descricao, f => f.Lorem.Sentence());

            _updateDtoFaker = new Faker<UpdateCategoriaDto>("pt_BR")
                .RuleFor(dto => dto.Nome, f => f.Commerce.Categories(1)[0])
                .RuleFor(dto => dto.Descricao, f => f.Lorem.Sentence());
        }

        [Fact]
        public async Task GetAllAsync_WhenNoCategoriasExist_ShouldReturnEmptyList()
        {
            _mockCategoriaRepo.Setup(repo => repo.GetAllAsync())
                               .ReturnsAsync(new List<Categoria>());

            var result = await _service.GetAllAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WhenCategoriasExist_ShouldReturnMappedDtoList()
        {
            var categorias = _categoriaFaker.Generate(3);
            _mockCategoriaRepo.Setup(repo => repo.GetAllAsync())
                               .ReturnsAsync(categorias);

            var result = await _service.GetAllAsync();

            result.Should().HaveCount(3);
            result.First().Nome.Should().Be(categorias.First().Nome);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCategoriaDoesNotExist_ShouldReturnNull()
        {
            var id = Guid.NewGuid();
            _mockCategoriaRepo.Setup(repo => repo.GetByIdAsync(id))
                              .ReturnsAsync((Categoria?)null);

            var result = await _service.GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WhenCategoriaExists_ShouldReturnMappedDto()
        {
            var categoria = _categoriaFaker.Generate();
            _mockCategoriaRepo.Setup(repo => repo.GetByIdAsync(categoria.Id))
                              .ReturnsAsync(categoria);

            var result = await _service.GetByIdAsync(categoria.Id);

            result.Should().NotBeNull();
            result.Id.Should().Be(categoria.Id);
            result.Nome.Should().Be(categoria.Nome);
        }

        [Fact]
        public async Task CreateAsync_WhenCategoriaNameAlreadyExists_ShouldReturnNull()
        {
            var dto = _createDtoFaker.Generate();
            var existingCategoria = _categoriaFaker.Generate();

            _mockCategoriaRepo.Setup(repo => repo.GetByNameAsync(dto.Nome))
                              .ReturnsAsync(existingCategoria);

            var result = await _service.CreateAsync(dto);

            result.Should().BeNull();
            _mockCategoriaRepo.Verify(repo => repo.AddAsync(It.IsAny<Categoria>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenNameIsUnique_ShouldAddAndReturnMappedDto()
        {
            var dto = _createDtoFaker.Generate();
            _mockCategoriaRepo.Setup(repo => repo.GetByNameAsync(dto.Nome))
                              .ReturnsAsync((Categoria?)null);

            var result = await _service.CreateAsync(dto);

            result.Should().NotBeNull();
            result.Nome.Should().Be(dto.Nome);
            result.Descricao.Should().Be(dto.Descricao);

            _mockCategoriaRepo.Verify(repo => repo.AddAsync(
                It.Is<Categoria>(c => c.Nome == dto.Nome && c.Descricao == dto.Descricao)
            ), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenCategoriaDoesNotExist_ShouldReturnNull()
        {
            var id = Guid.NewGuid();
            var dto = _updateDtoFaker.Generate();
            _mockCategoriaRepo.Setup(repo => repo.GetByIdAsync(id))
                              .ReturnsAsync((Categoria?)null);

            var result = await _service.UpdateAsync(id, dto);

            result.Should().BeNull();
            _mockCategoriaRepo.Verify(repo => repo.UpdateAsync(It.IsAny<Categoria>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenCategoriaExists_ShouldUpdateAndReturnMappedDto()
        {
            var categoria = _categoriaFaker.Generate();
            var dto = _updateDtoFaker.Generate();

            _mockCategoriaRepo.Setup(repo => repo.GetByIdAsync(categoria.Id))
                              .ReturnsAsync(categoria);

            var result = await _service.UpdateAsync(categoria.Id, dto);

            result.Should().NotBeNull();
            result.Id.Should().Be(categoria.Id);
            result.Nome.Should().Be(dto.Nome);
            result.Descricao.Should().Be(dto.Descricao);

            _mockCategoriaRepo.Verify(repo => repo.UpdateAsync(
                It.Is<Categoria>(c => c.Id == categoria.Id && c.Nome == dto.Nome)
            ), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenCategoriaDoesNotExist_ShouldReturnFalse()
        {
            var id = Guid.NewGuid();
            _mockCategoriaRepo.Setup(repo => repo.GetByIdAsync(id))
                              .ReturnsAsync((Categoria?)null);

            var result = await _service.DeleteAsync(id);

            result.Should().BeFalse();
            _mockCategoriaRepo.Verify(repo => repo.DeleteAsync(It.IsAny<Categoria>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenCategoriaExists_ShouldDeleteAndReturnTrue()
        {
            var categoria = _categoriaFaker.Generate();
            _mockCategoriaRepo.Setup(repo => repo.GetByIdAsync(categoria.Id))
                              .ReturnsAsync(categoria);

            var result = await _service.DeleteAsync(categoria.Id);

            result.Should().BeTrue();
            _mockCategoriaRepo.Verify(repo => repo.DeleteAsync(categoria), Times.Once);
        }
    }
}
