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
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISeguidorRepository _seguidorRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository, ISeguidorRepository seguidorRepository)
        {
            _usuarioRepository = usuarioRepository;
            _seguidorRepository = seguidorRepository;
        }

        public async Task<UsuarioProfileDto?> GetProfileByUsernameAsync(string username)
        {
            var usuario = await _usuarioRepository.GetByUsernameAsync(username);
            if (usuario == null) return null;

            // Mapear a entidade para o DTO de perfil
            return new UsuarioProfileDto
            {
                Id = usuario.Id,
                Username = usuario.NomeUsuario,
                NomeCompleto = usuario.NomeCompleto,
                Bio = usuario.Bio,
                UrlFotoPerfil = usuario.UrlFotoPerfil,
                ContagemSeguidores = usuario.Seguidores?.Count ?? 0,
                ContagemSeguindo = usuario.Seguindo?.Count ?? 0
            };
        }

        public async Task<UsuarioProfileDto?> UpdateProfileAsync(Guid userId, UpdateUsuarioDto dto)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario == null) return null;

            // Atualiza as propriedades do usuário com os dados do DTO
            usuario.NomeCompleto = dto.NomeCompleto;
            usuario.Bio = dto.Bio;
            usuario.UrlFotoPerfil = dto.UrlFotoPerfil;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _usuarioRepository.UpdateAsync(usuario);

            // Retorna o perfil atualizado
            return await GetProfileByUsernameAsync(usuario.NomeUsuario);
        }

        public async Task<IEnumerable<UsuarioProfileDto>> SearchUsersAsync(string query)
        {
            var usuarios = await _usuarioRepository.SearchByUsernameAsync(query);
            // Mapear a lista de entidades para uma lista de DTOs
            return usuarios.Select(u => new UsuarioProfileDto
            {
                Id = u.Id,
                Username = u.NomeUsuario,
                NomeCompleto = u.NomeCompleto,
                UrlFotoPerfil = u.UrlFotoPerfil,
                ContagemSeguidores = u.Seguidores?.Count ?? 0,
                ContagemSeguindo = u.Seguindo?.Count ?? 0
            });
        }

        public async Task<bool> FollowUserAsync(Guid seguidorId, Guid seguidoId)
        {
            // Regra de negócio: um usuário не pode seguir a si mesmo
            if (seguidorId == seguidoId) return false;

            // Verifica se o usuário a ser seguido existe
            var userToFollow = await _usuarioRepository.GetByIdAsync(seguidoId);
            if (userToFollow == null) return false;

            // Verifica se já não segue o usuário
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
            if (existingFollow == null) return false; // Não seguia, não há o que fazer

            await _seguidorRepository.DeleteAsync(existingFollow);
            return true;
        }
    }
}
