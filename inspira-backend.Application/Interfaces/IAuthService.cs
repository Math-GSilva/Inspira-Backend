using inspira_backend.Application.DTOs;

namespace inspira_backend.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto request);
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    }
}
