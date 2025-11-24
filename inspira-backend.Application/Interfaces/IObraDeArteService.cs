using inspira_backend.Application.DTOs;

namespace inspira_backend.Application.Interfaces
{
    public interface IObraDeArteService
    {
        Task<ObraDeArteResponseDto?> CreateAsync(CreateObraDeArteDto dto, Guid userId);
        Task<PaginatedResponseDto<ObraDeArteResponseDto>> GetAllAsync(Guid userId, Guid? categoriaId, int pageSize, string? cursor);
        Task<ObraDeArteResponseDto?> GetByIdAsync(Guid id);
        Task<ObraDeArteResponseDto?> UpdateAsync(Guid id, UpdateObraDeArteDto dto, Guid userId);
        Task<IEnumerable<ObraDeArteResponseDto>> GetAllByUserAsync(Guid userId);
        Task<bool> DeleteAsync(Guid id);
        Task<(byte[]? Data, string? ContentType)> GetMidiaByIdAsync(Guid id);
    }
}
