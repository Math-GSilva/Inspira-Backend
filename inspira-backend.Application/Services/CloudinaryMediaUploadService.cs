using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using inspira_backend.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace inspira_backend.Application.Services
{
    public class CloudinaryMediaUploadService : IMediaUploadService
    {
        private readonly ICloudinaryWrapper _cloudinary;

        public CloudinaryMediaUploadService(ICloudinaryWrapper cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var resourceType = GetResourceType(file.ContentType);

            await using var stream = file.OpenReadStream();

            UploadResult uploadResult;

            switch (resourceType)
            {
                case ResourceType.Image:
                    var imageParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                    };
                    uploadResult = await _cloudinary.UploadAsync(imageParams);
                    break;

                case ResourceType.Video:
                    var videoParams = new VideoUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                    };
                    uploadResult = await _cloudinary.UploadAsync(videoParams);
                    break;

                default:
                    throw new InvalidOperationException("Tipo de recurso não suportado para upload.");
            }

            if (uploadResult.Error != null)
            {
                return null;
            }

            return uploadResult.SecureUrl.ToString();
        }

        private ResourceType GetResourceType(string contentType)
        {
            string type = contentType.ToLower();

            if (type.StartsWith("image/"))
            {
                return ResourceType.Image;
            }

            if (type.StartsWith("video/") || type.StartsWith("audio/"))
            {
                return ResourceType.Video;
            }

            throw new ArgumentException("Tipo de conteúdo não suportado.");
        }
    }
}
