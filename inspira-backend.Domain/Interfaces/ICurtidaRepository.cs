using inspira_backend.Domain.Entities;

namespace inspira_backend.Domain.Interfaces
{
    public interface ICurtidaRepository
    {
        Task<Curtida?> GetByUserAndArtAsync(Guid userId, Guid obraDeArteId);
        Task<int> CountByObraDeArteIdAsync(Guid obraDeArteId);
        Task AddAsync(Curtida curtida);
        Task DeleteAsync(Curtida curtida);
    }
}
