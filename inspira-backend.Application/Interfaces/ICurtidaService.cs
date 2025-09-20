using inspira_backend.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.Interfaces
{
    public interface ICurtidaService
    {
        Task<CurtidaStatusDto?> CurtirAsync(Guid obraDeArteId, Guid userId);
        Task<CurtidaStatusDto?> DescurtirAsync(Guid obraDeArteId, Guid userId);
    }
}
