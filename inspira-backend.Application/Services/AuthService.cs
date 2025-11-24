using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;

namespace inspira_backend.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IUsuarioRepository usuarioRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _usuarioRepository = usuarioRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto request)
        {
            if (await _usuarioRepository.GetByUsernameAsync(request.Username) != null)
            {
                return null;
            }

            if (await _usuarioRepository.GetByEmailAsync(request.Email) != null)
            {
                return null;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var usuario = new Usuario
            {
                NomeCompleto = request.CompleteName,
                NomeUsuario = request.Username,
                Email = request.Email,
                SenhaHash = passwordHash,
                TipoUsuario = request.Role,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            await _usuarioRepository.AddAsync(usuario);

            return await LoginAsync(new LoginRequestDto { Username = request.Username, Password = request.Password });
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var usuario = await _usuarioRepository.GetByUsernameAsync(request.Username);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.SenhaHash))
            {
                return null;
            }

            var token = _jwtTokenGenerator.GenerateToken(usuario);

            return new LoginResponseDto
            {
                UserId = usuario.Id,
                Username = usuario.NomeUsuario,
                Token = token
            };
        }
    }
}
