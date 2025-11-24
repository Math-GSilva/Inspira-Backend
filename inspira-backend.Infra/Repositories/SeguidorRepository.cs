using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace inspira_backend.Infra.Repositories
{
    public class SeguidorRepository : ISeguidorRepository
    {
        private readonly InspiraDbContext _context;

        public SeguidorRepository(InspiraDbContext context)
        {
            _context = context;
        }

        public async Task<Seguidor?> GetByFollowerAndFollowedAsync(Guid seguidorId, Guid seguidoId)
        {
            return await _context.Seguidores
                .FirstOrDefaultAsync(s => s.SeguidorId == seguidorId && s.SeguidoId == seguidoId);
        }

        public async Task AddAsync(Seguidor seguidor)
        {
            await _context.Seguidores.AddAsync(seguidor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Seguidor seguidor)
        {
            _context.Seguidores.Remove(seguidor);
            await _context.SaveChangesAsync();
        }
    }
}
