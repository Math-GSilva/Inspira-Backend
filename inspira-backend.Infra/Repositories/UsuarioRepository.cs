using inspira_backend.Application.Interfaces;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Infra.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly InspiraDbContext _context;

        public UsuarioRepository(InspiraDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> GetByIdAsync(Guid id)
        {
            return await _context.Usuarios
                .Include(u => u.Seguidores)
                .ThenInclude(s => s.SeguidorUsuario)
                .Include(u => u.Seguindo)
                .ThenInclude(s => s.SeguidoUsuario)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario?> GetByUsernameAsync(string username)
        {
            return await _context.Usuarios
                .Include(u => u.Seguidores)
                .Include(u => u.Seguindo)
                .FirstOrDefaultAsync(u => u.NomeUsuario == username);
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<Usuario>> SearchByUsernameAsync(string username, Guid userId)
        {
            return await _context.Usuarios
                .Include(u => u.Seguidores)
                .Include(u => u.Seguindo)
                .Where(u => u.NomeUsuario.Contains(username) && u.Id != userId)
                .ToListAsync();
        }

        public async Task AddAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
}
