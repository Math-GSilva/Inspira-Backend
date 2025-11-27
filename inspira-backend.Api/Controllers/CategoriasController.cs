using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace inspira_backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _service;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(ICategoriaService service, ILogger<CategoriasController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoriaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Iniciando busca de todas as categorias.");

            try
            {
                var categorias = await _service.GetAllAsync();
                var count = categorias.Count();

                _logger.LogInformation("Busca concluída. Total de categorias encontradas: {Count}", count);
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao buscar categorias.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao buscar as categorias." });
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            _logger.LogInformation("Buscando categoria pelo ID: {Id}", id);

            try
            {
                var categoria = await _service.GetByIdAsync(id);
                if (categoria == null)
                {
                    _logger.LogWarning("Categoria não encontrada para o ID: {Id}", id);
                    return NotFound(new { message = "Categoria não encontrada." });
                }

                _logger.LogInformation("Categoria encontrada: {Nome} (ID: {Id})", categoria.Nome, categoria.Id);
                return Ok(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao buscar categoria por ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao buscar a categoria." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateCategoriaDto dto)
        {
            _logger.LogInformation("Tentativa de criação de nova categoria: {Nome}", dto.Nome);

            try
            {
                var categoria = await _service.CreateAsync(dto);
                if (categoria == null)
                {
                    _logger.LogWarning("Falha ao criar categoria. Já existe uma categoria com o nome '{Nome}'.", dto.Nome);
                    return BadRequest(new { message = "Uma categoria com este nome já existe." });
                }

                _logger.LogInformation("Categoria criada com sucesso. ID: {Id} | Nome: {Nome}", categoria.Id, categoria.Nome);
                return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao criar categoria: {Nome}", dto.Nome);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao criar a categoria." });
            }
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoriaDto dto)
        {
            _logger.LogInformation("Iniciando atualização da categoria ID: {Id}", id);

            try
            {
                var categoria = await _service.UpdateAsync(id, dto);
                if (categoria == null)
                {
                    _logger.LogWarning("Falha na atualização. Categoria não encontrada para o ID: {Id}", id);
                    return NotFound(new { message = "Categoria não encontrada." });
                }

                _logger.LogInformation("Categoria atualizada com sucesso. ID: {Id} | Novo Nome: {Nome}", categoria.Id, categoria.Nome);
                return Ok(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao atualizar categoria ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao atualizar a categoria." });
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Tentativa de exclusão da categoria ID: {Id}", id);

            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Falha na exclusão. Categoria não encontrada para o ID: {Id}", id);
                    return NotFound(new { message = "Categoria não encontrada." });
                }

                _logger.LogInformation("Categoria excluída com sucesso. ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao excluir categoria ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao excluir a categoria." });
            }
        }
    }
}