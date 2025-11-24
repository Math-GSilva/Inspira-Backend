using inspira_backend.Domain.Entities;

namespace inspira_backend.Domain.Interfaces
{
    public interface ISeguidorRepository
    {
        Task<Seguidor?> GetByFollowerAndFollowedAsync(Guid seguidorId, Guid seguidoId);

        Task AddAsync(Seguidor seguidor);

        Task DeleteAsync(Seguidor seguidor);
    }
}
