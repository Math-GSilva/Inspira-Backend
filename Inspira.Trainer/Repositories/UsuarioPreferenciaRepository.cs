using Inspira.Trainer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inspira.Trainer.Repositories
{
    public class UsuarioPreferenciaRepository : IUsuarioPreferenciaRepository
    {
        private readonly TrainingDbContext _context;
        private readonly ILogger<UsuarioPreferenciaRepository> _logger;

        public UsuarioPreferenciaRepository(TrainingDbContext context, ILogger<UsuarioPreferenciaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task BatchUpdatePreferenciasAsync(Guid usuarioId, IEnumerable<UsuarioPreferenciaCategoria> novasPreferencias)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.Set<UsuarioPreferenciaCategoria>()
                        .Where(p => p.UsuarioId == usuarioId)
                        .ExecuteDeleteAsync();

                    await _context.Set<UsuarioPreferenciaCategoria>().AddRangeAsync(novasPreferencias);

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Falha ao atualizar scores para o usuário {usuarioId}. Transação revertida.");
                    throw;
                }
            }
        }
    }
}
