using inspira_backend.Domain.Entities;

namespace inspira_backend.Domain.Interfaces
{
    public interface IComentarioRepository
    {
        Task<Comentario?> GetByIdAsync(Guid id);
        Task<IEnumerable<Comentario>> GetByObraDeArteIdAsync(Guid obraDeArteId);
        Task AddAsync(Comentario comentario);
        Task UpdateAsync(Comentario comentario);
        Task DeleteAsync(Comentario comentario);
    }
}
