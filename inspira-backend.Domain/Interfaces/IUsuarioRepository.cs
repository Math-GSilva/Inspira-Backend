using inspira_backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByIdAsync(Guid id);
        Task<Usuario?> GetByUsernameAsync(string username);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<IEnumerable<Usuario>> SearchByUsernameAsync(string username);
        Task AddAsync(Usuario usuario);
        Task UpdateAsync(Usuario usuario);
        // O método Delete pode ser adicionado se necessário
    }
}
