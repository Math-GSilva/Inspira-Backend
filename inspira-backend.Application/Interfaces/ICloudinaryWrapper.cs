using CloudinaryDotNet.Actions;

namespace inspira_backend.Application.Interfaces
{
    public interface ICloudinaryWrapper
    {
        Task<ImageUploadResult> UploadAsync(ImageUploadParams parameters);
        Task<VideoUploadResult> UploadAsync(VideoUploadParams parameters);
    }
}
