using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.Interfaces
{
    public interface IMediaUploadService
    {
        Task<string?> UploadAsync(IFormFile file);
    }
}
