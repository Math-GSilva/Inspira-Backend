using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IEnumerable<Usuario>> SearchAsync(string? query, Guid? categoriaPrincipal, Guid userId)
        {
            var usuariosQuery = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                usuariosQuery = usuariosQuery.Where(u =>
                    u.NomeUsuario.Contains(query) || u.NomeCompleto.Contains(query));
            }

            if (categoriaPrincipal.HasValue)
            {
                usuariosQuery = usuariosQuery.Where(u => u.CategoriaPrincipalId == categoriaPrincipal);
            }

            return await usuariosQuery
                .Where(u => u.Id != userId)
                .Include(u => u.Seguidores)
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
