using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace inspira_backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(IUsuarioService usuarioService, ILogger<UsuarioController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim != null ? Guid.Parse(claim) : Guid.Empty;
        }

        [HttpGet("search")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<UsuarioProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] string? categoriaPrincipal)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Usuário {UserId} realizou uma pesquisa. Termo: '{Query}' | Categoria: '{CategoriaId}'",
                userId, query ?? "N/A", categoriaPrincipal ?? "N/A");

            if (string.IsNullOrWhiteSpace(query) && string.IsNullOrWhiteSpace(categoriaPrincipal))
            {
                _logger.LogWarning("Tentativa de pesquisa inválida (sem parâmetros) pelo usuário {UserId}.", userId);
                return BadRequest(new { message = "É necessário fornecer um termo de pesquisa ou uma categoria." });
            }

            try
            {
                Guid? idCategoriaPrincipal = string.IsNullOrWhiteSpace(categoriaPrincipal) ? null : Guid.Parse(categoriaPrincipal);
                var usuarios = await _usuarioService.SearchUsersAsync(query, idCategoriaPrincipal, userId);

                var count = usuarios.Count();
                _logger.LogInformation("Pesquisa concluída. {Count} usuários encontrados.", count);

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao pesquisar usuários. Termo: {Query}", query);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao realizar a pesquisa." });
            }
        }

        [HttpGet("{username}")]
        [ProducesResponseType(typeof(UsuarioProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfile(string username)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Visualizando perfil do usuário: {TargetUsername}. Solicitado por: {RequestingUserId}", username, userId);

            try
            {
                var profile = await _usuarioService.GetProfileByUsernameAsync(username, userId);
                if (profile == null)
                {
                    _logger.LogWarning("Perfil não encontrado: {TargetUsername}", username);
                    return NotFound(new { message = "Usuário não encontrado." });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar perfil do usuário {TargetUsername}", username);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao carregar o perfil." });
            }
        }

        [HttpPut("me")]
        [Authorize]
        [ProducesResponseType(typeof(UsuarioProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMyProfile([FromForm] UpdateUsuarioDto dto)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Usuário {UserId} iniciou atualização de perfil.", userId);

            try
            {
                var updatedProfile = await _usuarioService.UpdateProfileAsync(userId, dto);
                if (updatedProfile == null)
                {
                    _logger.LogWarning("Falha ao atualizar perfil: Usuário {UserId} não encontrado no banco.", userId);
                    return NotFound(new { message = "Usuário não encontrado para atualização." });
                }

                _logger.LogInformation("Perfil do usuário {UserId} atualizado com sucesso.", userId);
                return Ok(updatedProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao atualizar perfil do usuário {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao atualizar o perfil." });
            }
        }

        [HttpPost("{id}/follow")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Follow(Guid id)
        {
            var seguidorId = GetCurrentUserId();
            _logger.LogInformation("Usuário {SeguidorId} solicitou seguir o usuário {SeguidoId}.", seguidorId, id);

            try
            {
                var result = await _usuarioService.FollowUserAsync(seguidorId, id);

                if (!result)
                {
                    _logger.LogWarning("Falha ao seguir: Usuário {SeguidoId} não encontrado ou operação inválida (ex: seguir a si mesmo).", id);
                    return BadRequest(new { message = "Não foi possível seguir o usuário." });
                }

                _logger.LogInformation("Usuário {SeguidorId} agora segue {SeguidoId}.", seguidorId, id);
                return Ok(new { message = "Usuário seguido com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico na operação Follow. Seguidor: {SeguidorId} | Seguido: {SeguidoId}", seguidorId, id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao tentar seguir o usuário." });
            }
        }

        [HttpDelete("{id}/follow")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Unfollow(Guid id)
        {
            var seguidorId = GetCurrentUserId();
            _logger.LogInformation("Usuário {SeguidorId} solicitou deixar de seguir {SeguidoId}.", seguidorId, id);

            try
            {
                var result = await _usuarioService.UnfollowUserAsync(seguidorId, id);

                if (!result)
                {
                    _logger.LogWarning("Falha ao deixar de seguir: Relação não existe entre {SeguidorId} e {SeguidoId}.", seguidorId, id);
                    return BadRequest(new { message = "Você não segue este usuário." });
                }

                _logger.LogInformation("Usuário {SeguidorId} deixou de seguir {SeguidoId}.", seguidorId, id);
                return Ok(new { message = "Deixou de seguir o usuário com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico na operação Unfollow. Seguidor: {SeguidorId} | Seguido: {SeguidoId}", seguidorId, id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao tentar deixar de seguir o usuário." });
            }
        }
    }
}