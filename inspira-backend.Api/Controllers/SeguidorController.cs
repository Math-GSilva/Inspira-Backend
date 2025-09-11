using inspira_backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace inspira_backend.API.Controllers
{
    [ApiController]
    [Route("api/usuarios/{usuarioId}")]
    public class SeguidoresController : ControllerBase
    {
        private readonly ISeguidorService _seguidorService;

        public SeguidoresController(ISeguidorService seguidorService)
        {
            _seguidorService = seguidorService;
        }

        [HttpGet("seguidores")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSeguidores(Guid usuarioId)
        {
            var seguidores = await _seguidorService.GetSeguidoresAsync(usuarioId);
            if (seguidores == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }
            return Ok(seguidores);
        }

        [HttpGet("seguindo")]
        [AllowAnonymous]
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
