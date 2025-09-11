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

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var usuarios = await _usuarioService.SearchUsersAsync(query);
            return Ok(usuarios);
        }

        [HttpGet("{username}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfile(string username)
        {
            var profile = await _usuarioService.GetProfileByUsernameAsync(username);
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
