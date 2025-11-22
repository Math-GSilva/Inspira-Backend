using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.Interfaces
{
    public interface ICloudinaryWrapper
    {
        Task<ImageUploadResult> UploadAsync(ImageUploadParams parameters);
        Task<VideoUploadResult> UploadAsync(VideoUploadParams parameters);
    }
}
