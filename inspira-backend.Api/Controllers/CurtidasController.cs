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
        private readonly ILogger<CurtidasController> _logger;

        public CurtidasController(ICurtidaService curtidaService, ILogger<CurtidasController> logger)
        {
            _curtidaService = curtidaService;
            _logger = logger;
        }

        private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        [HttpPost]
        [ProducesResponseType(typeof(CurtidaStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Curtir([FromBody] CreateCurtidaDto dto)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Usuário {UserId} solicitou curtir a obra {ObraId}.", userId, dto.ObraDeArteId);

            try
            {
                var result = await _curtidaService.CurtirAsync(dto.ObraDeArteId, userId);
                if (result == null)
                {
                    _logger.LogWarning("Falha ao curtir: Obra {ObraId} não encontrada.", dto.ObraDeArteId);
                    return NotFound(new { message = "Obra de arte não encontrada." });
                }

                _logger.LogInformation("Curtida registrada com sucesso. Obra: {ObraId} | Usuário: {UserId} | Total atual: {TotalCurtidas}",
                    dto.ObraDeArteId, userId, result.TotalCurtidas);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao processar curtida na obra {ObraId} pelo usuário {UserId}.", dto.ObraDeArteId, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar a curtida." });
            }
        }

        [HttpDelete("{obraDeArteId:guid}")]
        [ProducesResponseType(typeof(CurtidaStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Descurtir(Guid obraDeArteId)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Usuário {UserId} solicitou remover curtida da obra {ObraId}.", userId, obraDeArteId);

            try
            {
                var result = await _curtidaService.DescurtirAsync(obraDeArteId, userId);
                if (result == null)
                {
                    _logger.LogWarning("Falha ao descurtir: Obra {ObraId} não encontrada.", obraDeArteId);
                    return NotFound(new { message = "Obra de arte não encontrada." });
                }

                _logger.LogInformation("Curtida removida com sucesso. Obra: {ObraId} | Usuário: {UserId} | Total atual: {TotalCurtidas}",
                    obraDeArteId, userId, result.TotalCurtidas);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao remover curtida da obra {ObraId} pelo usuário {UserId}.", obraDeArteId, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao remover a curtida." });
            }
        }
    }
}