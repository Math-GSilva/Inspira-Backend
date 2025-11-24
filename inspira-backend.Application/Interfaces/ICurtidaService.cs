using inspira_backend.Application.DTOs;

namespace inspira_backend.Application.Interfaces
{
    public interface ICurtidaService
    {
        Task<CurtidaStatusDto?> CurtirAsync(Guid obraDeArteId, Guid userId);
        Task<CurtidaStatusDto?> DescurtirAsync(Guid obraDeArteId, Guid userId);
    }
}
