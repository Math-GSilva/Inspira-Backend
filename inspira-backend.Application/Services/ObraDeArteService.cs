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
    public class ObraDeArteService : IObraDeArteService
    {
        private readonly IObraDeArteRepository _obraDeArteRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IMediaUploadService _mediaUploadService;

        public ObraDeArteService(IObraDeArteRepository obraDeArteRepository, IUsuarioRepository usuarioRepository,
            ICategoriaRepository categoriaRepository, IMediaUploadService mediaUploadService)
        {
            _obraDeArteRepository = obraDeArteRepository;
            _usuarioRepository = usuarioRepository;
            _categoriaRepository = categoriaRepository;
            _mediaUploadService = mediaUploadService;
        }

        private ObraDeArteResponseDto MapToDto(ObraDeArte obra, Guid? userId = null)
        {
            return new ObraDeArteResponseDto
            {
                Id = obra.Id,
                Titulo = obra.Titulo,
                Descricao = obra.Descricao,
                DataPublicacao = obra.DataPublicacao,
                AutorUsername = obra.Usuario?.NomeUsuario ?? "N/A",
                UrlFotoPerfilAutor = obra.Usuario?.UrlFotoPerfil ?? "",
                CategoriaNome = obra.Categoria?.Nome ?? "N/A",
                TotalCurtidas = obra.Curtidas?.Count ?? 0,
                Url = obra.UrlMidia,
                TipoConteudoMidia = obra.TipoConteudoMidia ?? "",
                CurtidaPeloUsuario = userId != null && (obra.Curtidas?.Any(curtida => curtida.UsuarioId == userId) ?? false )
            };
        }

        public async Task<ObraDeArteResponseDto?> CreateAsync(CreateObraDeArteDto dto, Guid userId)
        {
            var autor = await _usuarioRepository.GetByIdAsync(userId);
            var categoria = await _categoriaRepository.GetByIdAsync(dto.CategoriaId);

            if (autor == null || categoria == null) return null;

            if (dto.Midia == null || dto.Midia.Length == 0 || !IsValidMediaType(dto.Midia.ContentType))
            {
                return null;
            }

            var mediaUrl = await _mediaUploadService.UploadAsync(dto.Midia);
            if (mediaUrl == null) return null;

            byte[] dadosMidia;
            using (var memoryStream = new MemoryStream())
            {
                await dto.Midia.CopyToAsync(memoryStream);
                dadosMidia = memoryStream.ToArray();
            }

            var obraDeArte = new ObraDeArte
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                UrlMidia = mediaUrl,
                TipoConteudoMidia = dto.Midia.ContentType,
                DataPublicacao = DateTime.UtcNow,
                UsuarioId = userId,
                CategoriaId = dto.CategoriaId
            };

            await _obraDeArteRepository.AddAsync(obraDeArte);
            obraDeArte.Usuario = autor;
            obraDeArte.Categoria = categoria;
            return MapToDto(obraDeArte);
        }

        public async Task<IEnumerable<ObraDeArteResponseDto>> GetAllAsync(Guid userId, Guid? categoriaId)
        {
            var obras = await _obraDeArteRepository.GetAllAsync(categoriaId);
            return obras.Select(obra => MapToDto(obra, userId));
        }

        public async Task<ObraDeArteResponseDto?> GetByIdAsync(Guid id)
        {
            var obra = await _obraDeArteRepository.GetByIdAsync(id);
            return obra == null ? null : MapToDto(obra);
        }

        public async Task<(byte[]?, string?)> GetMidiaByIdAsync(Guid id)
        {
            var obra = await _obraDeArteRepository.GetByIdAsync(id, includeMediaData: true);
            if (obra?.DadosMidia == null || obra.TipoConteudoMidia == null)
            {
                return (null, null);
            }
            return (obra.DadosMidia, obra.TipoConteudoMidia);
        }

        public async Task<ObraDeArteResponseDto?> UpdateAsync(Guid id, UpdateObraDeArteDto dto, Guid userId)
        {
            var obra = await _obraDeArteRepository.GetByIdAsync(id, includeMediaData: true);
            if (obra == null) return null;
            if (obra.UsuarioId != userId) throw new UnauthorizedAccessException("Apenas o autor pode editar a obra.");

            obra.Titulo = dto.Titulo;
            obra.Descricao = dto.Descricao;

            await _obraDeArteRepository.UpdateAsync(obra);
            return MapToDto(obra);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var obra = await _obraDeArteRepository.GetByIdAsync(id);
            if (obra == null) return false;
            if (obra.UsuarioId != userId) throw new UnauthorizedAccessException("Apenas o autor pode apagar a obra.");

            await _obraDeArteRepository.DeleteAsync(obra);
            return true;
        }

        private bool IsValidMediaType(string contentType)
        {
            string type = contentType.ToLower();
            return type.StartsWith("image/") ||
                   type.StartsWith("video/") ||
                   type.StartsWith("audio/");
        }
    }
}
