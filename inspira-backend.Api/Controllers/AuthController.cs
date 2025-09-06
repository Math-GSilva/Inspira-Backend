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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var response = await _authService.RegisterAsync(request);
            if (response == null)
            {
                return BadRequest(new { message = "Usuário ou e-mail já existente." });
            }
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var response = await _authService.LoginAsync(request);
            if (response == null)
            {
                return Unauthorized(new { message = "Credenciais inválidas." });
            }
            return Ok(response);
        }
    }
}
