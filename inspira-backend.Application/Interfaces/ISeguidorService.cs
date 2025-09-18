using inspira_backend.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.Interfaces
{
    public interface ISeguidorService
    {
        Task<IEnumerable<SeguidorResumoDto>?> GetSeguidoresAsync(Guid usuarioId);
        Task<IEnumerable<SeguidorResumoDto>?> GetSeguindoAsync(Guid usuarioId);
    }
}
