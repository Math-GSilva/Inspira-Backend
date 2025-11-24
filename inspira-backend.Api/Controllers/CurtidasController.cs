using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace inspira_backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CurtidasController : ControllerBase
    {
        private readonly ICurtidaService _curtidaService;

        public CurtidasController(ICurtidaService curtidaService)
        {
            _curtidaService = curtidaService;
        }

        private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

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
