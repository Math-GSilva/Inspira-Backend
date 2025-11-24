using inspira_backend.Application.DTOs;

namespace inspira_backend.Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<UsuarioProfileDto?> GetProfileByUsernameAsync(string username, Guid userId);
        Task<UsuarioProfileDto?> UpdateProfileAsync(Guid userId, UpdateUsuarioDto dto);
        Task<IEnumerable<UsuarioProfileDto>> SearchUsersAsync(string? query, Guid? categoriaPrincipal, Guid currentUserId);
        Task<bool> FollowUserAsync(Guid seguidorId, Guid seguidoId);
        Task<bool> UnfollowUserAsync(Guid seguidorId, Guid seguidoId);
    }
}
