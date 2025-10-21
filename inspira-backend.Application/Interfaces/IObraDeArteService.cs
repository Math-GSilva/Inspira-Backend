using inspira_backend.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.Interfaces
{
    public interface IObraDeArteService
    {
        Task<ObraDeArteResponseDto?> CreateAsync(CreateObraDeArteDto dto, Guid userId);
        Task<IEnumerable<ObraDeArteResponseDto>> GetAllAsync(Guid userId);
        Task<ObraDeArteResponseDto?> GetByIdAsync(Guid id);
        Task<ObraDeArteResponseDto?> UpdateAsync(Guid id, UpdateObraDeArteDto dto, Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid userId);
        Task<(byte[]? Data, string? ContentType)> GetMidiaByIdAsync(Guid id);
    }
}
