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
    public class ObraDeArteRepository : IObraDeArteRepository
    {
        private readonly InspiraDbContext _context;

        public ObraDeArteRepository(InspiraDbContext context)
        {
            _context = context;
        }

        public async Task<ObraDeArte?> GetByIdAsync(Guid id, bool includeMediaData = false)
        {
            var query = _context.ObrasDeArte
                .Include(o => o.Usuario)
                .Include(o => o.Categoria)
                .Include(o => o.Curtidas)
                .AsQueryable();

            if (!includeMediaData)
            {
                query = query.Select(o => new ObraDeArte
                {
                    Id = o.Id,
                    Titulo = o.Titulo,
                    Descricao = o.Descricao,
                    DataPublicacao = o.DataPublicacao,
                    UsuarioId = o.UsuarioId,
                    Usuario = o.Usuario,
                    CategoriaId = o.CategoriaId,
                    Categoria = o.Categoria,
                    Curtidas = o.Curtidas,
                    UrlMidia = o.UrlMidia,
                    TipoConteudoMidia = o.TipoConteudoMidia
                });
            }

            return await query.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<ObraDeArte>> GetAllAsync(Guid? categoriaId = null) // 1. Adiciona o parâmetro opcional
        {
            IQueryable<ObraDeArte> query = _context.ObrasDeArte
                .Include(o => o.Usuario)
                .Include(o => o.Categoria)
                .Include(o => o.Curtidas)
                .OrderByDescending(o => o.DataPublicacao);

            if (categoriaId.HasValue)
            {
                query = query.Where(o => o.CategoriaId == categoriaId.Value);
            }

            return await query
                .Select(o => new ObraDeArte
                {
                    Id = o.Id,
                    Titulo = o.Titulo,
                    Descricao = o.Descricao,
                    DataPublicacao = o.DataPublicacao,
                    UsuarioId = o.UsuarioId,
                    Usuario = o.Usuario,
                    CategoriaId = o.CategoriaId,
                    Categoria = o.Categoria,
                    Curtidas = o.Curtidas,
                    UrlMidia = o.UrlMidia,
                    TipoConteudoMidia = o.TipoConteudoMidia
                })
                .ToListAsync();
        }

        public async Task AddAsync(ObraDeArte obraDeArte)
        {
            await _context.ObrasDeArte.AddAsync(obraDeArte);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ObraDeArte obraDeArte)
        {
            _context.ObrasDeArte.Update(obraDeArte);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ObraDeArte obraDeArte)
        {
            _context.ObrasDeArte.Remove(obraDeArte);
            await _context.SaveChangesAsync();
        }
    }
}
