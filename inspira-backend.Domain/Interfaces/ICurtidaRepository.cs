using inspira_backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
