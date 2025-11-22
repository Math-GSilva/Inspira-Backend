using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using inspira_backend.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.Services
{
    public class CloudinaryWrapper : ICloudinaryWrapper
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryWrapper(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public Task<ImageUploadResult> UploadAsync(ImageUploadParams parameters)
        {
            return _cloudinary.UploadAsync(parameters);
        }

        public Task<VideoUploadResult> UploadAsync(VideoUploadParams parameters)
        {
            return _cloudinary.UploadAsync(parameters);
        }
    }
}
