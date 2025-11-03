using Inspira.Trainer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Usa uma transação para garantir consistência
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Apaga todos os scores antigos deste usuário
                    await _context.Set<UsuarioPreferenciaCategoria>()
                        .Where(p => p.UsuarioId == usuarioId)
                        .ExecuteDeleteAsync();

                    // 2. Insere os novos scores
                    await _context.Set<UsuarioPreferenciaCategoria>().AddRangeAsync(novasPreferencias);

                    // 3. Salva as mudanças
                    await _context.SaveChangesAsync();

                    // 4. Confirma a transação
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    // Se algo der errado, desfaz tudo
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Falha ao atualizar scores para o usuário {usuarioId}. Transação revertida.");
                    throw;
                }
            }
        }
    }
}
