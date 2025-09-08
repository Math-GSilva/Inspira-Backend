using inspira_backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace inspira_backend.API.Controllers
{
    [ApiController]
    // Rotas aninhadas sob 'usuarios' para melhor semântica
    [Route("api/usuarios/{usuarioId}")]
    public class SeguidoresController : ControllerBase
    {
        private readonly ISeguidorService _seguidorService;

        public SeguidoresController(ISeguidorService seguidorService)
        {
            _seguidorService = seguidorService;
        }

        // GET: api/usuarios/{usuarioId}/seguidores
        [HttpGet("seguidores")]
        [AllowAnonymous] // Geralmente, a lista de seguidores é pública
        public async Task<IActionResult> GetSeguidores(Guid usuarioId)
        {
            var seguidores = await _seguidorService.GetSeguidoresAsync(usuarioId);
            if (seguidores == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }
            return Ok(seguidores);
        }

        // GET: api/usuarios/{usuarioId}/seguindo
        [HttpGet("seguindo")]
        [AllowAnonymous] // Geralmente, a lista de quem se segue é pública
        public async Task<IActionResult> GetSeguindo(Guid usuarioId)
        {
            var seguindo = await _seguidorService.GetSeguindoAsync(usuarioId);
            if (seguindo == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }
            return Ok(seguindo);
        }
    }
}
