using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace inspira_backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            _logger.LogInformation("Iniciando registro de novo usuário: {Username} | Email: {Email}", request.Username, request.Email);

            try
            {
                var response = await _authService.RegisterAsync(request);

                if (response == null)
                {
                    _logger.LogWarning("Falha no registro: Username '{Username}' ou Email '{Email}' já existem.", request.Username, request.Email);
                    return BadRequest(new { message = "Usuário ou e-mail já existente." });
                }

                _logger.LogInformation("Usuário registrado com sucesso. ID: {UserId} | Username: {Username}", response.UserId, response.Username);

                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao tentar registrar o usuário: {Username}", request.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar seu registro." });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            _logger.LogInformation("Tentativa de login para o usuário: {Username}", request.Username);

            try
            {
                var response = await _authService.LoginAsync(request);

                if (response == null)
                {
                    _logger.LogWarning("Falha no login: Credenciais inválidas para {Username}", request.Username);
                    return Unauthorized(new { message = "Credenciais inválidas." });
                }

                _logger.LogInformation("Login efetuado com sucesso para o usuário: {Username}", request.Username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico durante o login do usuário: {Username}", request.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao realizar o login." });
            }
        }
    }
}