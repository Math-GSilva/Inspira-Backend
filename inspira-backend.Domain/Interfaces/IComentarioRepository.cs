using inspira_backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
