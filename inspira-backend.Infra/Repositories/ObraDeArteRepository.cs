using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            int? lastIsLiked,
            double? lastScore,
            DateTime? lastDate)
        {
            IQueryable<ObraDeArte> query = _context.ObrasDeArte
                .Include(o => o.Usuario)
                .Include(o => o.Categoria)
                .Include(o => o.Curtidas); //

            if (categoriaId.HasValue)
            {
                query = query.Where(o => o.CategoriaId == categoriaId.Value);
            }

            var scoredQuery = query.Select(o => new
            {
                Obra = o,
                Score = _context.Set<UsuarioPreferenciaCategoria>()
                    .Where(p => p.UsuarioId == userId && p.CategoriaId == o.CategoriaId)
                    .Select(p => p.Score)
                    .FirstOrDefault(),
                IsLikedByCurrentUser = o.Curtidas.Any(c => c.UsuarioId == userId) ? 1 : 0
            });

            if (lastIsLiked.HasValue && lastScore.HasValue && lastDate.HasValue)
            {
                scoredQuery = scoredQuery.Where(x =>
                    (x.IsLikedByCurrentUser > lastIsLiked.Value) ||
                    (x.IsLikedByCurrentUser == lastIsLiked.Value && x.Score < lastScore.Value) ||
                    (x.IsLikedByCurrentUser == lastIsLiked.Value && x.Score == lastScore.Value && x.Obra.DataPublicacao < lastDate.Value)
                );
            }

            var pagedResults = await scoredQuery
                .OrderBy(x => x.IsLikedByCurrentUser)
                .ThenByDescending(x => x.Score)
                .ThenByDescending(x => x.Obra.DataPublicacao)
                .Take(pageSize + 1)
                .ToListAsync();

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
