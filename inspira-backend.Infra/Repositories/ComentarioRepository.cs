using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace inspira_backend.Infra.Repositories
{
    public class ComentarioRepository : IComentarioRepository
    {
        private readonly InspiraDbContext _context;

        public ComentarioRepository(InspiraDbContext context) { _context = context; }

        public async Task<Comentario?> GetByIdAsync(Guid id)
        {
            return await _context.Comentarios
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Comentario>> GetByObraDeArteIdAsync(Guid obraDeArteId)
        {
            return await _context.Comentarios
                .Where(c => c.ObraDeArteId == obraDeArteId && c.ComentarioPaiId == null)
                .Include(c => c.Usuario)
                .Include(c => c.Respostas)
                    .ThenInclude(r => r.Usuario)
                .OrderByDescending(c => c.DataComentario)
                .ToListAsync();
        }

        public async Task AddAsync(Comentario comentario)
        {
            await _context.Comentarios.AddAsync(comentario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Comentario comentario)
        {
            _context.Comentarios.Update(comentario);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Comentario comentario)
        {
            _context.Comentarios.Remove(comentario);
            await _context.SaveChangesAsync();
        }
    }
}
