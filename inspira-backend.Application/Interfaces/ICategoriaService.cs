using inspira_backend.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.Interfaces
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaResponseDto>> GetAllAsync();
        Task<CategoriaResponseDto?> GetByIdAsync(Guid id);
        Task<CategoriaResponseDto?> CreateAsync(CreateCategoriaDto dto);
        Task<CategoriaResponseDto?> UpdateAsync(Guid id, UpdateCategoriaDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
