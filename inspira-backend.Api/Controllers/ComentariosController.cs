using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace inspira_backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComentariosController : ControllerBase
    {
        private readonly IComentarioService _comentarioService;

        public ComentariosController(IComentarioService comentarioService)
        {
            _comentarioService = comentarioService;
        }

        private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private IEnumerable<string> GetCurrentUserRoles() => User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        // GET: api/comentarios?obraDeArteId={id}
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetComentarios([FromQuery] Guid obraDeArteId)
        {
            var comentarios = await _comentarioService.GetComentariosByObraDeArteIdAsync(obraDeArteId);
            return Ok(comentarios);
        }

        // POST: api/comentarios
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Criar([FromBody] CreateComentarioDto dto)
        {
            var comentario = await _comentarioService.CriarComentarioAsync(dto, GetCurrentUserId());
            if (comentario == null)
            {
                return NotFound(new { message = "Obra de arte não encontrada." });
            }
            return Ok(comentario);
        }

        // DELETE: api/comentarios/{id}
        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _comentarioService.DeleteComentarioAsync(id, GetCurrentUserId(), GetCurrentUserRoles());
                if (!result) return NotFound("Comentário não encontrado.");
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
