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
    public class CurtidaRepository : ICurtidaRepository
    {
        private readonly InspiraDbContext _context;

        public CurtidaRepository(InspiraDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Curtida curtida)
        {
            await _context.Curtidas.AddAsync(curtida);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Curtida curtida)
        {
            _context.Curtidas.Remove(curtida);
            await _context.SaveChangesAsync();
        }

        public async Task<Curtida?> GetByUserAndArtAsync(Guid userId, Guid obraDeArteId)
        {
            return await _context.Curtidas
                .FirstOrDefaultAsync(c => c.UsuarioId == userId && c.ObraDeArteId == obraDeArteId);
        }

        public async Task<int> CountByObraDeArteIdAsync(Guid obraDeArteId)
        {
            return await _context.Curtidas.CountAsync(c => c.ObraDeArteId == obraDeArteId);
        }
    }
}
