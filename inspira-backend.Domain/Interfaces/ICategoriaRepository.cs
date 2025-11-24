using inspira_backend.Domain.Entities;

namespace inspira_backend.Domain.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<Categoria?> GetByIdAsync(Guid id);
        Task<Categoria?> GetByNameAsync(string name);
        Task<IEnumerable<Categoria>> GetAllAsync();
        Task AddAsync(Categoria categoria);
        Task UpdateAsync(Categoria categoria);
        Task DeleteAsync(Categoria categoria);
    }
}
