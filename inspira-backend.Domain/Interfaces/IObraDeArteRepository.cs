using inspira_backend.Domain.Entities;

namespace inspira_backend.Domain.Interfaces
{
    public interface IObraDeArteRepository
    {
        Task<ObraDeArte?> GetByIdAsync(Guid id, bool includeMediaData = false);
        Task<List<(ObraDeArte Obra, int IsLiked, double Score)>> GetAllAsync(Guid userId, Guid? categoriaId, int pageSize, int? lastIsLiked, double? lastScore, DateTime? lastDate);
        Task<IEnumerable<ObraDeArte>> GetAllByUserAsync(Guid usuarioId);
        Task AddAsync(ObraDeArte obraDeArte);
        Task UpdateAsync(ObraDeArte obraDeArte);
        Task DeleteAsync(ObraDeArte obraDeArte);
    }
}
