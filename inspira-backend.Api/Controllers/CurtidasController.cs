using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace inspira_backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Todas as ações de curtida exigem que o utilizador esteja autenticado
    public class CurtidasController : ControllerBase
    {
        private readonly ICurtidaService _curtidaService;

        public CurtidasController(ICurtidaService curtidaService)
        {
            _curtidaService = curtidaService;
        }

        private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // POST: api/curtidas
        // Ação de "Curtir" uma obra de arte
        [HttpPost]
        public async Task<IActionResult> Curtir([FromBody] CreateCurtidaDto dto)
        {
            var result = await _curtidaService.CurtirAsync(dto.ObraDeArteId, GetCurrentUserId());
            if (result == null)
            {
                return NotFound(new { message = "Obra de arte não encontrada." });
            }
            return Ok(result);
        }

        // DELETE: api/curtidas/{obraDeArteId}
        // Ação de "Descurtir" uma obra de arte
        [HttpDelete("{obraDeArteId:guid}")]
        public async Task<IActionResult> Descurtir(Guid obraDeArteId)
        {
            var result = await _curtidaService.DescurtirAsync(obraDeArteId, GetCurrentUserId());
            if (result == null)
            {
                return NotFound(new { message = "Obra de arte não encontrada." });
            }
            return Ok(result);
        }
    }
}
