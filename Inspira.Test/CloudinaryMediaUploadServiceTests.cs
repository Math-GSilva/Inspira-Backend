using CloudinaryDotNet.Actions;
using FluentAssertions;
using inspira_backend.Application.Interfaces;
using inspira_backend.Application.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspira.Test
{
    public class CloudinaryMediaUploadServiceTests
    {
        private readonly Mock<ICloudinaryWrapper> _mockWrapper;
        private readonly CloudinaryMediaUploadService _service;

        public CloudinaryMediaUploadServiceTests()
        {
            _mockWrapper = new Mock<ICloudinaryWrapper>();
            _service = new CloudinaryMediaUploadService(_mockWrapper.Object);
        }

        [Fact]
        public async Task UploadAsync_WhenFileIsNull_ShouldReturnNull()
        {
            var result = await _service.UploadAsync(null);
            result.Should().BeNull();
        }

        [Fact]
        public async Task UploadAsync_WhenFileIsEmpty_ShouldReturnNull()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            var result = await _service.UploadAsync(mockFile.Object);
            result.Should().BeNull();
        }

        [Fact]
        public async Task UploadAsync_WhenContentTypeInvalid_ShouldThrowArgumentException()
        {
            var mockFile = CreateMockFile("test.pdf", "application/pdf");

            Func<Task> act = async () => await _service.UploadAsync(mockFile.Object);

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("Tipo de conteúdo não suportado.");
        }

        [Fact]
        public async Task UploadAsync_WhenImageUploadSuccess_ShouldReturnUrl()
        {
            var mockFile = CreateMockFile("foto.png", "image/png");
            var expectedUrl = "https://cloudinary.com/minha-foto.png";

            var successResult = new ImageUploadResult
            {
                SecureUrl = new Uri(expectedUrl)
            };

            _mockWrapper.Setup(w => w.UploadAsync(It.IsAny<ImageUploadParams>()))
                        .ReturnsAsync(successResult);

            var result = await _service.UploadAsync(mockFile.Object);

            result.Should().Be(expectedUrl);

            _mockWrapper.Verify(w => w.UploadAsync(It.IsAny<ImageUploadParams>()), Times.Once);
            _mockWrapper.Verify(w => w.UploadAsync(It.IsAny<VideoUploadParams>()), Times.Never);
        }

        [Fact]
        public async Task UploadAsync_WhenVideoUploadSuccess_ShouldReturnUrl()
        {
            var mockFile = CreateMockFile("video.mp4", "video/mp4");
            var expectedUrl = "https://cloudinary.com/meu-video.mp4";

            var successResult = new VideoUploadResult
            {
                SecureUrl = new Uri(expectedUrl)
            };

            _mockWrapper.Setup(w => w.UploadAsync(It.IsAny<VideoUploadParams>()))
                        .ReturnsAsync(successResult);

            var result = await _service.UploadAsync(mockFile.Object);

            result.Should().Be(expectedUrl);
            _mockWrapper.Verify(w => w.UploadAsync(It.IsAny<VideoUploadParams>()), Times.Once);
        }

        [Fact]
        public async Task UploadAsync_WhenCloudinaryReturnsError_ShouldReturnNull()
        {
            var mockFile = CreateMockFile("foto.jpg", "image/jpeg");

            var errorResult = new ImageUploadResult
            {
                Error = new Error { Message = "Server unavailable" }
            };

            _mockWrapper.Setup(w => w.UploadAsync(It.IsAny<ImageUploadParams>()))
                        .ReturnsAsync(errorResult);

            var result = await _service.UploadAsync(mockFile.Object);

            result.Should().BeNull();
        }

        private Mock<IFormFile> CreateMockFile(string fileName, string contentType)
        {
            var mock = new Mock<IFormFile>();
            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.ContentType).Returns(contentType);
            mock.Setup(f => f.Length).Returns(1024);
            mock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
            return mock;
        }
    }
}
