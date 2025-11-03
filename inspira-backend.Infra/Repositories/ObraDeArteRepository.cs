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

        public async Task<List<(ObraDeArte Obra, int IsLiked, double Score)>> GetAllAsync(
            Guid userId,
            Guid? categoriaId,
            int pageSize,
            int? lastIsLiked,     // <-- ADICIONADO
            double? lastScore,
            DateTime? lastDate)
        {
            // 1. Começa com a query base de ObrasDeArte, já incluindo os dados
            IQueryable<ObraDeArte> query = _context.ObrasDeArte
                .Include(o => o.Usuario)
                .Include(o => o.Categoria)
                .Include(o => o.Curtidas); //

            // 2. Aplica o filtro de categoria (se houver)
            if (categoriaId.HasValue)
            {
                query = query.Where(o => o.CategoriaId == categoriaId.Value);
            }

            // 3. Faz o "LEFT JOIN" com a tabela de scores E checa se foi curtido
            var scoredQuery = query.Select(o => new
            {
                Obra = o,
                Score = _context.Set<UsuarioPreferenciaCategoria>()
                    .Where(p => p.UsuarioId == userId && p.CategoriaId == o.CategoriaId)
                    .Select(p => p.Score)
                    .FirstOrDefault(), // Retorna 0.0 (default) se não houver score

                // --- CAMPO ADICIONADO ---
                // (Usamos 0 para 'não curtido' e 1 para 'curtido' para facilitar a ordenação)
                IsLikedByCurrentUser = o.Curtidas.Any(c => c.UsuarioId == userId) ? 1 : 0
            });

            // 4. Aplica a lógica do cursor composto (paginação de 3 chaves)
            if (lastIsLiked.HasValue && lastScore.HasValue && lastDate.HasValue)
            {
                // A lógica de paginação SQL para (IsLiked ASC, Score DESC, Data DESC) é:
                // WHERE (IsLiked > lastIsLiked) OR
                //       (IsLiked = lastIsLiked AND Score < lastScore) OR
                //       (IsLiked = lastIsLiked AND Score = lastScore AND Data < lastDate)

                scoredQuery = scoredQuery.Where(x =>
                    (x.IsLikedByCurrentUser > lastIsLiked.Value) ||
                    (x.IsLikedByCurrentUser == lastIsLiked.Value && x.Score < lastScore.Value) ||
                    (x.IsLikedByCurrentUser == lastIsLiked.Value && x.Score == lastScore.Value && x.Obra.DataPublicacao < lastDate.Value)
                );
            }

            // 5. Aplica a ordenação, paginação e executa a query
            var pagedResults = await scoredQuery
                .OrderBy(x => x.IsLikedByCurrentUser) // <-- 1. Ordena por 'não curtido' primeiro
                .ThenByDescending(x => x.Score)       // <-- 2. Ordena pelo score da IA
                .ThenByDescending(x => x.Obra.DataPublicacao) // <-- 3. Desempata pela data
                .Take(pageSize + 1) // Pega +1 para saber se "HasMoreItems"
                .ToListAsync();

            // 6. Retorna a lista de tuplas (Obra, IsLiked, Score)
            return pagedResults.Select(r => (r.Obra, r.IsLikedByCurrentUser, r.Score)).ToList();
        }

        public async Task<IEnumerable<ObraDeArte>> GetAllByUserAsync(Guid usuarioId)
        {
            IQueryable<ObraDeArte> query = _context.ObrasDeArte
                .Include(o => o.Usuario)
                .Include(o => o.Categoria)
                .Include(o => o.Curtidas)
                .Where(o => o.UsuarioId == usuarioId)
                .OrderByDescending(o => o.DataPublicacao);

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
