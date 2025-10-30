using inspira_backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Domain.Interfaces
{
    public interface IObraDeArteRepository
    {
        Task<ObraDeArte?> GetByIdAsync(Guid id, bool includeMediaData = false);
        Task<List<ObraDeArte>> GetAllAsync(Guid? categoriaId, int pageSize, DateTime? cursor);
        Task<IEnumerable<ObraDeArte>> GetAllByUserAsync(Guid usuarioId);
        Task AddAsync(ObraDeArte obraDeArte);
        Task UpdateAsync(ObraDeArte obraDeArte);
        Task DeleteAsync(ObraDeArte obraDeArte);
    }
}
