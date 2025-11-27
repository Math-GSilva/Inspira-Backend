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
        private readonly ILogger<ComentariosController> _logger;

        public ComentariosController(IComentarioService comentarioService, ILogger<ComentariosController> logger)
        {
            _comentarioService = comentarioService;
            _logger = logger;
        }

        private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private IEnumerable<string> GetCurrentUserRoles() => User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ComentarioResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetComentarios([FromQuery] Guid obraDeArteId)
        {
            _logger.LogInformation("Buscando comentários da obra ID: {ObraId}", obraDeArteId);

            try
            {
                var comentarios = await _comentarioService.GetComentariosByObraDeArteIdAsync(obraDeArteId);
                var count = comentarios.Count();

                _logger.LogInformation("Busca concluída. {Count} comentários encontrados para a obra {ObraId}.", count, obraDeArteId);
                return Ok(comentarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao buscar comentários da obra {ObraId}.", obraDeArteId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao carregar os comentários." });
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ComentarioResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Criar([FromBody] CreateComentarioDto dto)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Usuário {UserId} tentando comentar na obra {ObraId}.", userId, dto.ObraDeArteId);

            try
            {
                var comentario = await _comentarioService.CriarComentarioAsync(dto, userId);
                if (comentario == null)
                {
                    _logger.LogWarning("Falha ao comentar: Obra de arte {ObraId} não encontrada.", dto.ObraDeArteId);
                    return NotFound(new { message = "Obra de arte não encontrada." });
                }

                _logger.LogInformation("Comentário criado com sucesso. ID: {ComentarioId} | Autor: {UserId}", comentario.Id, userId);

                return StatusCode(StatusCodes.Status201Created, comentario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao criar comentário na obra {ObraId} pelo usuário {UserId}.", dto.ObraDeArteId, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao salvar o comentário." });
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Usuário {UserId} solicitou a exclusão do comentário {ComentarioId}.", userId, id);

            try
            {
                var result = await _comentarioService.DeleteComentarioAsync(id, userId, GetCurrentUserRoles());

                if (!result)
                {
                    _logger.LogWarning("Tentativa de exclusão falhou: Comentário {ComentarioId} não encontrado.", id);
                    return NotFound(new { message = "Comentário não encontrado." });
                }

                _logger.LogInformation("Comentário {ComentarioId} excluído com sucesso pelo usuário {UserId}.", id, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Acesso negado: Usuário {UserId} tentou apagar o comentário {ComentarioId} sem permissão.", userId, id);
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao excluir comentário {ComentarioId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao excluir o comentário." });
            }
        }
    }
}