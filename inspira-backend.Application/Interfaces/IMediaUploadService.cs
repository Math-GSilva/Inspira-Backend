using Microsoft.AspNetCore.Http;

namespace inspira_backend.Application.Interfaces
{
    public interface IMediaUploadService
    {
        Task<string?> UploadAsync(IFormFile file);
    }
}
