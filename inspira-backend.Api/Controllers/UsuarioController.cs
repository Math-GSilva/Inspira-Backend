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

        // GET: api/usuarios/search?query=nome
        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var usuarios = await _usuarioService.SearchUsersAsync(query);
            return Ok(usuarios);
        }

        // GET: api/usuarios/nomedousuario
        [HttpGet("{username}")]
        [AllowAnonymous] // Qualquer um pode ver um perfil
        public async Task<IActionResult> GetProfile(string username)
        {
            var profile = await _usuarioService.GetProfileByUsernameAsync(username);
            if (profile == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }
            return Ok(profile);
        }

        // PUT: api/usuarios/me
        [HttpPut("me")]
        [Authorize] // O usuário precisa estar logado para atualizar seu perfil
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUsuarioDto dto)
        {
            // Pega o ID do usuário a partir do token JWT
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var updatedProfile = await _usuarioService.UpdateProfileAsync(userId, dto);
            if (updatedProfile == null)
            {
                return NotFound(new { message = "Usuário não encontrado para atualização." });
            }
            return Ok(updatedProfile);
        }

        // POST: api/usuarios/{id}/follow
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

        // DELETE: api/usuarios/{id}/follow
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
