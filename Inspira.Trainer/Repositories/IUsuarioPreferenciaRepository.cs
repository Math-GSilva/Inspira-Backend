using Inspira.Trainer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspira.Trainer.Repositories
{
    public interface IUsuarioPreferenciaRepository
    {
        /// <summary>
        /// Apaga todos os scores antigos de um usuário e insere os novos.
        /// </summary>
        Task BatchUpdatePreferenciasAsync(Guid usuarioId, IEnumerable<UsuarioPreferenciaCategoria> novasPreferencias);
    }
}
