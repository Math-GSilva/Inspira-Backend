using Inspira.Trainer.Data;

namespace Inspira.Trainer.Repositories
{
    public interface IUsuarioPreferenciaRepository
    {
        Task BatchUpdatePreferenciasAsync(Guid usuarioId, IEnumerable<UsuarioPreferenciaCategoria> novasPreferencias);
    }
}
