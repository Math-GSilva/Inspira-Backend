using inspira_backend.Application.DTOs;

namespace inspira_backend.Application.Interfaces
{
    public interface ISeguidorService
    {
        Task<IEnumerable<SeguidorResumoDto>?> GetSeguidoresAsync(Guid usuarioId);
        Task<IEnumerable<SeguidorResumoDto>?> GetSeguindoAsync(Guid usuarioId);
    }
}
