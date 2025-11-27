using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace inspira_backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObrasDeArteController : ControllerBase
    {
        private readonly IObraDeArteService _service;
        private readonly ILogger<ObrasDeArteController> _logger;

        public ObrasDeArteController(IObraDeArteService service, ILogger<ObrasDeArteController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim != null ? Guid.Parse(claim) : Guid.Empty;
        }

        [HttpPost]
        [Authorize(Roles = "Artista, Administrador")]
        [RequestSizeLimit(104857600)]
        [ProducesResponseType(typeof(ObraDeArteResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromForm] CreateObraDeArteDto dto)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Iniciando upload de nova obra. Título: {Titulo} | Categoria: {CategoriaId} | Usuário: {UserId}",
                dto.Titulo, dto.CategoriaId, userId);

            try
            {
                var obraCriada = await _service.CreateAsync(dto, userId);
                if (obraCriada == null)
                {
                    _logger.LogWarning("Falha na criação da obra. Dados inválidos ou erro no upload. Usuário: {UserId}", userId);
                    return BadRequest("Dados inválidos para a criação da obra ou falha no upload.");
                }

                _logger.LogInformation("Obra criada com sucesso. ID: {ObraId} | Título: {Titulo}", obraCriada.Id, obraCriada.Titulo);
                return CreatedAtAction(nameof(GetById), new { id = obraCriada.Id }, obraCriada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao criar obra de arte. Usuário: {UserId} | Título: {Titulo}", userId, dto.Titulo);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar o upload da obra." });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedResponseDto<ObraDeArteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? categoriaId,
            [FromQuery] string? cursor,
            [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Buscando feed de obras. Categoria: {CategoriaId} | Cursor: {Cursor} | Usuário solicitante: {UserId}",
                categoriaId, cursor, userId);

            try
            {
                var paginatedObras = await _service.GetAllAsync(userId, categoriaId, pageSize, cursor);

                _logger.LogInformation("Feed carregado. Itens retornados: {Count} | Próximo cursor: {NextCursor}",
                    paginatedObras.Items.Count, paginatedObras.NextCursor);

                return Ok(paginatedObras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao carregar feed de obras.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao carregar o feed." });
            }
        }

        [HttpGet("user/{userId:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ObraDeArteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetArtworksByUserId(Guid userId)
        {
            _logger.LogInformation("Buscando portfólio do usuário: {TargetUserId}", userId);

            try
            {
                var obras = await _service.GetAllByUserAsync(userId);
                var count = obras.Count();

                _logger.LogInformation("Portfólio carregado. Obras encontradas: {Count} para o usuário {TargetUserId}", count, userId);
                return Ok(obras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao buscar obras do usuário {TargetUserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao carregar as obras do usuário." });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ObraDeArteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            _logger.LogInformation("Visualizando detalhes da obra ID: {ObraId}", id);

            try
            {
                var obra = await _service.GetByIdAsync(id);
                if (obra == null)
                {
                    _logger.LogWarning("Obra não encontrada: {ObraId}", id);
                    return NotFound();
                }

                return Ok(obra);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar detalhes da obra {ObraId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao buscar a obra." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Artista, Administrador")]
        [ProducesResponseType(typeof(ObraDeArteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateObraDeArteDto dto)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Iniciando atualização da obra {ObraId} pelo usuário {UserId}.", id, userId);

            try
            {
                var obraAtualizada = await _service.UpdateAsync(id, dto, userId);
                if (obraAtualizada == null)
                {
                    _logger.LogWarning("Falha na atualização: Obra {ObraId} não encontrada.", id);
                    return NotFound();
                }

                _logger.LogInformation("Obra {ObraId} atualizada com sucesso.", id);
                return Ok(obraAtualizada);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Acesso negado: Usuário {UserId} tentou editar obra {ObraId} sem permissão.", userId, id);
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao atualizar obra {ObraId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Erro interno ao atualizar a obra." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Artista, Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Solicitação de exclusão da obra {ObraId} pelo usuário {UserId}.", id, userId);

            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Falha na exclusão: Obra {ObraId} não encontrada.", id);
                    return NotFound();
                }

                _logger.LogInformation("Obra {ObraId} excluída com sucesso.", id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Acesso negado na exclusão da obra {ObraId} pelo usuário {UserId}.", id, userId);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao excluir obra {ObraId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Erro interno ao excluir a obra." });
            }
        }
    }
}