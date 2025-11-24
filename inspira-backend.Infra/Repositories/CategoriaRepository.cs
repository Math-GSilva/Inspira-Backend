using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace inspira_backend.Infra.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly InspiraDbContext _context;

        public CategoriaRepository(InspiraDbContext context)
        {
            _context = context;
        }

        public async Task<Categoria?> GetByIdAsync(Guid id)
        {
            return await _context.Categorias.FindAsync(id);
        }

        public async Task<Categoria?> GetByNameAsync(string name)
        {
            return await _context.Categorias.FirstOrDefaultAsync(c => c.Nome == name);
        }

        public async Task<IEnumerable<Categoria>> GetAllAsync()
        {
            return await _context.Categorias.ToListAsync();
        }

        public async Task AddAsync(Categoria categoria)
        {
            await _context.Categorias.AddAsync(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Categoria categoria)
        {
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Categoria categoria)
        {
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
        }
    }
}
