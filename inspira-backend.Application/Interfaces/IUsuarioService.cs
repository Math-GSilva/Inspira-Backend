using inspira_backend.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<UsuarioProfileDto?> GetProfileByUsernameAsync(string username);
        Task<UsuarioProfileDto?> UpdateProfileAsync(Guid userId, UpdateUsuarioDto dto);
        Task<IEnumerable<UsuarioProfileDto>> SearchUsersAsync(string query, Guid userId);
        Task<bool> FollowUserAsync(Guid seguidorId, Guid seguidoId);
        Task<bool> UnfollowUserAsync(Guid seguidorId, Guid seguidoId);
    }
}
