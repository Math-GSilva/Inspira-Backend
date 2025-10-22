using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;

namespace inspira_backend.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISeguidorRepository _seguidorRepository;
        private readonly IMediaUploadService _mediaUploadService;

        public UsuarioService(IUsuarioRepository usuarioRepository, ISeguidorRepository seguidorRepository, IMediaUploadService mediaUploadService)
        {
            _usuarioRepository = usuarioRepository;
            _seguidorRepository = seguidorRepository;
            _mediaUploadService = mediaUploadService;
        }

        public async Task<UsuarioProfileDto?> GetProfileByUsernameAsync(string username, Guid userId)
        {
            var usuario = await _usuarioRepository.GetByUsernameAsync(username);
            if (usuario == null) return null;

            return new UsuarioProfileDto
            {
                Id = usuario.Id,
                Username = usuario.NomeUsuario,
                NomeCompleto = usuario.NomeCompleto,
                Bio = usuario.Bio,
                UrlFotoPerfil = usuario.UrlFotoPerfil,
                ContagemSeguidores = usuario.Seguidores?.Count ?? 0,
                ContagemSeguindo = usuario.Seguindo?.Count ?? 0,
                SeguidoPeloUsuarioAtual = usuario.Seguidores?.Any(seguidor => seguidor.SeguidorId.Equals(userId)) ?? false
            };
        }

        public async Task<UsuarioProfileDto?> UpdateProfileAsync(Guid userId, UpdateUsuarioDto dto)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario == null) return null;

            var urlFotoPerfil = "";
            if (dto.FotoPerfil.Length > 0)
                urlFotoPerfil = await _mediaUploadService.UploadAsync(dto.FotoPerfil);

            usuario.NomeCompleto = dto.NomeCompleto;
            usuario.Bio = dto.Bio;
            usuario.DataAtualizacao = DateTime.UtcNow;
            if(string.IsNullOrWhiteSpace(urlFotoPerfil))
                usuario.UrlFotoPerfil = urlFotoPerfil;

            await _usuarioRepository.UpdateAsync(usuario);

            return await GetProfileByUsernameAsync(usuario.NomeUsuario, userId);
        }

        public async Task<IEnumerable<UsuarioProfileDto>> SearchUsersAsync(string? query, Guid? categoriaPrincipal, Guid userId)
        {
            var usuarios = await _usuarioRepository.SearchAsync(query, categoriaPrincipal, userId);

            return usuarios.Select(u => new UsuarioProfileDto
            {
                Id = u.Id,
                Username = u.NomeUsuario,
                NomeCompleto = u.NomeCompleto,
                UrlFotoPerfil = u.UrlFotoPerfil,
                SeguidoPeloUsuarioAtual = u.Seguidores?.Any(seguidor => seguidor.SeguidorId.Equals(userId)) ?? false
            });
        }

        public async Task<bool> FollowUserAsync(Guid seguidorId, Guid seguidoId)
        {
            if (seguidorId == seguidoId) return false;

            var userToFollow = await _usuarioRepository.GetByIdAsync(seguidoId);
            if (userToFollow == null) return false;

            var existingFollow = await _seguidorRepository.GetByFollowerAndFollowedAsync(seguidorId, seguidoId);
            if (existingFollow != null) return true; // Já segue, operação bem-sucedida

            var seguidor = new Seguidor
            {
                SeguidorId = seguidorId,
                SeguidoId = seguidoId
            };

            await _seguidorRepository.AddAsync(seguidor);
            return true;
        }

        public async Task<bool> UnfollowUserAsync(Guid seguidorId, Guid seguidoId)
        {
            var existingFollow = await _seguidorRepository.GetByFollowerAndFollowedAsync(seguidorId, seguidoId);
            if (existingFollow == null) return false; // Não segue, não pode deixar de seguir

            await _seguidorRepository.DeleteAsync(existingFollow);
            return true;
        }
    }
}
