using Bogus;
using FluentAssertions;
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
    public class CurtidaServiceTests
    {
        private readonly Mock<ICurtidaRepository> _mockCurtidaRepo;
        private readonly Mock<IObraDeArteRepository> _mockObraRepo;
        private readonly ICurtidaService _service;
        private readonly Faker<ObraDeArte> _obraFaker;

        public CurtidaServiceTests()
        {
            _mockCurtidaRepo = new Mock<ICurtidaRepository>();
            _mockObraRepo = new Mock<IObraDeArteRepository>();
            _service = new CurtidaService(_mockCurtidaRepo.Object, _mockObraRepo.Object);

            _obraFaker = new Faker<ObraDeArte>()
                .RuleFor(o => o.Id, f => Guid.NewGuid());
        }

        [Fact]
        public async Task CurtirAsync_WhenObraDeArteDoesNotExist_ShouldReturnNull()
        {
            var obraId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockObraRepo.Setup(repo => repo.GetByIdAsync(obraId, false))
                         .ReturnsAsync((ObraDeArte?)null);

            var result = await _service.CurtirAsync(obraId, userId);

            result.Should().BeNull();
            _mockCurtidaRepo.Verify(repo => repo.AddAsync(It.IsAny<Curtida>()), Times.Never);
        }

        [Fact]
        public async Task CurtirAsync_WhenNotAlreadyLiked_ShouldAddLikeAndReturnStatus()
        {
            var obra = _obraFaker.Generate();
            var userId = Guid.NewGuid();
            var newTotalLikes = 5;

            _mockObraRepo.Setup(repo => repo.GetByIdAsync(obra.Id, false))
                         .ReturnsAsync(obra);

            _mockCurtidaRepo.Setup(repo => repo.GetByUserAndArtAsync(userId, obra.Id))
                            .ReturnsAsync((Curtida?)null);

            _mockCurtidaRepo.Setup(repo => repo.CountByObraDeArteIdAsync(obra.Id))
                            .ReturnsAsync(newTotalLikes);

            var result = await _service.CurtirAsync(obra.Id, userId);

            result.Should().NotBeNull();
            result.Curtiu.Should().BeTrue();
            result.TotalCurtidas.Should().Be(newTotalLikes);

            _mockCurtidaRepo.Verify(repo => repo.AddAsync(
                It.Is<Curtida>(c => c.UsuarioId == userId && c.ObraDeArteId == obra.Id)
            ), Times.Once);
        }

        [Fact]
        public async Task CurtirAsync_WhenAlreadyLiked_ShouldNotAddLikeAndReturnStatus()
        {
            var obra = _obraFaker.Generate();
            var userId = Guid.NewGuid();
            var existingLike = new Curtida { UsuarioId = userId, ObraDeArteId = obra.Id };
            var totalLikes = 10;

            _mockObraRepo.Setup(repo => repo.GetByIdAsync(obra.Id, false))
                         .ReturnsAsync(obra);

            _mockCurtidaRepo.Setup(repo => repo.GetByUserAndArtAsync(userId, obra.Id))
                            .ReturnsAsync(existingLike);

            _mockCurtidaRepo.Setup(repo => repo.CountByObraDeArteIdAsync(obra.Id))
                            .ReturnsAsync(totalLikes);

            var result = await _service.CurtirAsync(obra.Id, userId);

            result.Should().NotBeNull();
            result.Curtiu.Should().BeTrue();
            result.TotalCurtidas.Should().Be(totalLikes);

            _mockCurtidaRepo.Verify(repo => repo.AddAsync(It.IsAny<Curtida>()), Times.Never);
        }

        [Fact]
        public async Task DescurtirAsync_WhenObraDeArteDoesNotExist_ShouldReturnNull()
        {
            var obraId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockObraRepo.Setup(repo => repo.GetByIdAsync(obraId, false))
                         .ReturnsAsync((ObraDeArte?)null);

            var result = await _service.DescurtirAsync(obraId, userId);

            result.Should().BeNull();
            _mockCurtidaRepo.Verify(repo => repo.DeleteAsync(It.IsAny<Curtida>()), Times.Never);
        }

        [Fact]
        public async Task DescurtirAsync_WhenAlreadyLiked_ShouldDeleteLikeAndReturnStatus()
        {
            var obra = _obraFaker.Generate();
            var userId = Guid.NewGuid();
            var existingLike = new Curtida { UsuarioId = userId, ObraDeArteId = obra.Id };
            var newTotalLikes = 4;

            _mockObraRepo.Setup(repo => repo.GetByIdAsync(obra.Id, false))
                         .ReturnsAsync(obra);

            _mockCurtidaRepo.Setup(repo => repo.GetByUserAndArtAsync(userId, obra.Id))
                            .ReturnsAsync(existingLike);

            _mockCurtidaRepo.Setup(repo => repo.CountByObraDeArteIdAsync(obra.Id))
                            .ReturnsAsync(newTotalLikes);

            var result = await _service.DescurtirAsync(obra.Id, userId);

            result.Should().NotBeNull();
            result.Curtiu.Should().BeFalse();
            result.TotalCurtidas.Should().Be(newTotalLikes);

            _mockCurtidaRepo.Verify(repo => repo.DeleteAsync(existingLike), Times.Once);
        }

        [Fact]
        public async Task DescurtirAsync_WhenNotLiked_ShouldNotDeleteAndReturnStatus()
        {
            var obra = _obraFaker.Generate();
            var userId = Guid.NewGuid();
            var totalLikes = 5;

            _mockObraRepo.Setup(repo => repo.GetByIdAsync(obra.Id, false))
                         .ReturnsAsync(obra);

            _mockCurtidaRepo.Setup(repo => repo.GetByUserAndArtAsync(userId, obra.Id))
                            .ReturnsAsync((Curtida?)null);

            _mockCurtidaRepo.Setup(repo => repo.CountByObraDeArteIdAsync(obra.Id))
                            .ReturnsAsync(totalLikes);

            var result = await _service.DescurtirAsync(obra.Id, userId);

            result.Should().NotBeNull();
            result.Curtiu.Should().BeFalse();
            result.TotalCurtidas.Should().Be(totalLikes);

            _mockCurtidaRepo.Verify(repo => repo.DeleteAsync(It.IsAny<Curtida>()), Times.Never);
        }
    }
}
