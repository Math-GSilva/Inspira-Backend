using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using inspira_backend.Domain.Interfaces;

namespace inspira_backend.Application.Services
{
    public class SeguidorService : ISeguidorService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public SeguidorService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<IEnumerable<SeguidorResumoDto>?> GetSeguidoresAsync(Guid usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
            {
                return null;
            }

            return usuario.Seguidores.Select(s => new SeguidorResumoDto
            {
                UsuarioId = s.SeguidorUsuario.Id,
                Username = s.SeguidorUsuario.NomeUsuario,
                NomeCompleto = s.SeguidorUsuario.NomeCompleto,
                UrlFotoPerfil = s.SeguidorUsuario.UrlFotoPerfil
            });
        }

        public async Task<IEnumerable<SeguidorResumoDto>?> GetSeguindoAsync(Guid usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
            {
                return null;
            }

            return usuario.Seguindo.Select(s => new SeguidorResumoDto
            {
                UsuarioId = s.SeguidoUsuario.Id,
                Username = s.SeguidoUsuario.NomeUsuario,
                NomeCompleto = s.SeguidoUsuario.NomeCompleto,
                UrlFotoPerfil = s.SeguidoUsuario.UrlFotoPerfil
            });
        }
    }
}
