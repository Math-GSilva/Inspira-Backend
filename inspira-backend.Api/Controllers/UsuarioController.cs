using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace inspira_backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        /// <summary>
        /// Pesquisa usuários com base em um termo de pesquisa (nome/username) e/ou uma categoria principal.
        /// </summary>
        /// <param name="query">Termo para pesquisar no nome de usuário ou nome completo.</param>
        /// <param name="categoriaPrincipal">Categoria principal para filtrar os usuários.</param>
        /// <returns>Uma lista de usuários que correspondem aos critérios de pesquisa.</returns>
        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] string? categoriaPrincipal)
        {
            // Validação para garantir que pelo menos um critério de pesquisa foi fornecido.
            if (string.IsNullOrWhiteSpace(query) && string.IsNullOrWhiteSpace(categoriaPrincipal))
            {
                return BadRequest(new { message = "É necessário fornecer um termo de pesquisa ou uma categoria." });
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // A chamada de serviço agora passa ambos os parâmetros.
            // A lógica de filtragem (LIKE no nome/username E/OU pela categoria) deve ser implementada no IUsuarioService.
            Guid? idCategoriaPrincipal = string.IsNullOrWhiteSpace(categoriaPrincipal) ? null : Guid.Parse(categoriaPrincipal);
            var usuarios = await _usuarioService.SearchUsersAsync(query, idCategoriaPrincipal, userId);

            return Ok(usuarios);
        }

        [HttpGet("{username}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfile(string username)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var profile = await _usuarioService.GetProfileByUsernameAsync(username, userId);
            if (profile == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }
            return Ok(profile);
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUsuarioDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var updatedProfile = await _usuarioService.UpdateProfileAsync(userId, dto);
            if (updatedProfile == null)
            {
                return NotFound(new { message = "Usuário não encontrado para atualização." });
            }
            return Ok(updatedProfile);
        }

        [HttpPost("{id}/follow")]
        [Authorize]
        public async Task<IActionResult> Follow(Guid id)
        {
            var seguidorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _usuarioService.FollowUserAsync(seguidorId, id);

            if (!result)
            {
                return BadRequest(new { message = "Não foi possível seguir o usuário." });
            }
            return Ok(new { message = "Usuário seguido com sucesso." });
        }

        [HttpDelete("{id}/follow")]
        [Authorize]
        public async Task<IActionResult> Unfollow(Guid id)
        {
            var seguidorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _usuarioService.UnfollowUserAsync(seguidorId, id);

            if (!result)
            {
                return BadRequest(new { message = "Você não segue este usuário." });
            }
            return Ok(new { message = "Deixou de seguir o usuário com sucesso." });
        }
    }
}
