using inspira_backend.Domain.Entities;

namespace inspira_backend.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Usuario usuario);
    }
}
