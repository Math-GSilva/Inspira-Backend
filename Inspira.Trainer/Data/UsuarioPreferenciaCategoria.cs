using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspira.Trainer.Data
{
    [Table("UsuarioPreferenciaCategoria")]
    public class UsuarioPreferenciaCategoria
    {
        public Guid UsuarioId { get; set; }
        public Guid CategoriaId { get; set; }
        public double Score { get; set; }
        public DateTime DataCalculo { get; set; } = DateTime.UtcNow; // O C# vai definir o default
    }
}
