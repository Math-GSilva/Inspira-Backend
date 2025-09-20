using inspira_backend.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.Interfaces
{
    public interface IComentarioService
    {
        Task<ComentarioResponseDto?> CriarComentarioAsync(CreateComentarioDto dto, Guid userId);
        Task<IEnumerable<ComentarioResponseDto>> GetComentariosByObraDeArteIdAsync(Guid obraDeArteId);
        Task<bool> DeleteComentarioAsync(Guid comentarioId, Guid userId, IEnumerable<string> userRoles);
    }
}
