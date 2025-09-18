using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepository;

        public CategoriaService(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        private CategoriaResponseDto MapToDto(Categoria categoria)
        {
            return new CategoriaResponseDto
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                Descricao = categoria.Descricao
            };
        }

        public async Task<IEnumerable<CategoriaResponseDto>> GetAllAsync()
        {
            var categorias = await _categoriaRepository.GetAllAsync();
            return categorias.Select(MapToDto);
        }

        public async Task<CategoriaResponseDto?> GetByIdAsync(Guid id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);
            return categoria == null ? null : MapToDto(categoria);
        }

        public async Task<CategoriaResponseDto?> CreateAsync(CreateCategoriaDto dto)
        {
            var existingCategoria = await _categoriaRepository.GetByNameAsync(dto.Nome);
            if (existingCategoria != null)
            {
                return null;
            }

            var categoria = new Categoria
            {
                // O Guid é gerado automaticamente pelo construtor da entidade
                Nome = dto.Nome,
                Descricao = dto.Descricao
            };

            await _categoriaRepository.AddAsync(categoria);
            return MapToDto(categoria);
        }

        public async Task<CategoriaResponseDto?> UpdateAsync(Guid id, UpdateCategoriaDto dto)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);
            if (categoria == null)
            {
                return null;
            }

            categoria.Nome = dto.Nome;
            categoria.Descricao = dto.Descricao;

            await _categoriaRepository.UpdateAsync(categoria);
            return MapToDto(categoria);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);
            if (categoria == null)
            {
                return false;
            }

            await _categoriaRepository.DeleteAsync(categoria);
            return true;
        }
    }
}
