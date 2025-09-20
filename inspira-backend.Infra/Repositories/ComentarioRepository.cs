using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Infra.Repositories
{
    public class ComentarioRepository : IComentarioRepository
    {
        private readonly InspiraDbContext _context;

        public ComentarioRepository(InspiraDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Comentario comentario)
        {
            await _context.Comentarios.AddAsync(comentario);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Comentario comentario)
        {
            _context.Comentarios.Remove(comentario);
            await _context.SaveChangesAsync();
        }

        public async Task<Comentario?> GetByIdAsync(int id)
        {
            return await _context.Comentarios.FindAsync(id);
        }
    }
}
