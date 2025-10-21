using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.Services
{
    public class ComentarioService : IComentarioService
    {
        private readonly IComentarioRepository _comentarioRepository;
        private readonly IObraDeArteRepository _obraDeArteRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public ComentarioService(IComentarioRepository comentarioRepository, IObraDeArteRepository obraDeArteRepository, IUsuarioRepository usuarioRepository)
        {
            _comentarioRepository = comentarioRepository;
            _obraDeArteRepository = obraDeArteRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<ComentarioResponseDto?> CriarComentarioAsync(CreateComentarioDto dto, Guid userId)
        {
            var obraDeArte = await _obraDeArteRepository.GetByIdAsync(dto.ObraDeArteId);
            if (obraDeArte == null) return null; // Obra não existe

            var comentario = new Comentario
            {
                Conteudo = dto.Conteudo,
                DataComentario = DateTime.UtcNow,
                UsuarioId = userId,
                ObraDeArteId = dto.ObraDeArteId,
            };

            await _comentarioRepository.AddAsync(comentario);

            comentario.Usuario = await _usuarioRepository.GetByIdAsync(userId);
            return MapToDto(comentario);
        }

        public async Task<IEnumerable<ComentarioResponseDto>> GetComentariosByObraDeArteIdAsync(Guid obraDeArteId)
        {
            var comentarios = await _comentarioRepository.GetByObraDeArteIdAsync(obraDeArteId);
            return MapComentariosToDto(comentarios);
        }

        public async Task<bool> DeleteComentarioAsync(Guid comentarioId, Guid userId, IEnumerable<string> userRoles)
        {
            var comentario = await _comentarioRepository.GetByIdAsync(comentarioId);
            if (comentario == null) return false;

            if (comentario.UsuarioId != userId && !userRoles.Contains("Administrador"))
                throw new UnauthorizedAccessException("Apenas o autor do comentário ou um administrador podem apagar.");

            await _comentarioRepository.DeleteAsync(comentario);
            return true;
        }

        private ComentarioResponseDto MapToDto(Comentario c) => new ComentarioResponseDto
        {
            Id = c.Id,
            Conteudo = c.Conteudo,
            DataComentario = c.DataComentario,
            AutorUsername = c.Usuario?.NomeUsuario ?? "N/A",
            AutorId = c.UsuarioId,
            Respostas = MapComentariosToDto(c.Respostas),
            UrlFotoPerfil = c.Usuario?.UrlFotoPerfil ?? ""
        };

        private List<ComentarioResponseDto> MapComentariosToDto(IEnumerable<Comentario> comentarios)
        {
            if (comentarios == null || !comentarios.Any())
                return new List<ComentarioResponseDto>();

            return comentarios.Select(MapToDto).ToList();
        }
    }
}
