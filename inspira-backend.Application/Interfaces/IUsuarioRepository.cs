using inspira_backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByUsernameAsync(string username);
        Task<Usuario?> GetByEmailAsync(string email);
        Task AddAsync(Usuario usuario);
        Task<Usuario?> GetByIdAsync(Guid id);
    }
}
