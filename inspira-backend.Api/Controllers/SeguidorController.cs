using inspira_backend.Application.DTOs;
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
        private readonly ILogger<SeguidoresController> _logger;

        public SeguidoresController(ISeguidorService seguidorService, ILogger<SeguidoresController> logger)
        {
            _seguidorService = seguidorService;
            _logger = logger;
        }

        [HttpGet("seguidores")]
        [ProducesResponseType(typeof(IEnumerable<SeguidorResumoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSeguidores(Guid usuarioId)
        {
            _logger.LogInformation("Iniciando busca de seguidores do usuário: {TargetUserId}", usuarioId);

            try
            {
                var seguidores = await _seguidorService.GetSeguidoresAsync(usuarioId);

                if (seguidores == null)
                {
                    _logger.LogWarning("Usuário não encontrado ao buscar seguidores: {TargetUserId}", usuarioId);
                    return NotFound(new { message = "Usuário não encontrado." });
                }

                var count = seguidores.Count();
                _logger.LogInformation("Busca de seguidores concluída. O usuário {TargetUserId} tem {Count} seguidores.", usuarioId, count);

                return Ok(seguidores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao buscar seguidores do usuário {TargetUserId}", usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao carregar a lista de seguidores." });
            }
        }

        [HttpGet("seguindo")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<SeguidorResumoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSeguindo(Guid usuarioId)
        {
            _logger.LogInformation("Buscando lista de quem o usuário {TargetUserId} está seguindo.", usuarioId);

            try
            {
                var seguindo = await _seguidorService.GetSeguindoAsync(usuarioId);

                if (seguindo == null)
                {
                    _logger.LogWarning("Usuário não encontrado ao buscar lista de seguindo: {TargetUserId}", usuarioId);
                    return NotFound(new { message = "Usuário não encontrado." });
                }

                var count = seguindo.Count();
                _logger.LogInformation("Busca concluída. O usuário {TargetUserId} segue {Count} perfis.", usuarioId, count);

                return Ok(seguindo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao buscar lista de seguindo do usuário {TargetUserId}", usuarioId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao carregar a lista de perfis seguidos." });
            }
        }
    }
}