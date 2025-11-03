using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Domain.Entities
{
    [Table("UsuarioPreferenciaCategoria")]
    public class UsuarioPreferenciaCategoria
    {
        public Guid UsuarioId { get; set; }
        public Guid CategoriaId { get; set; }
        public double Score { get; set; }
        public DateTime DataCalculo { get; set; }

    }
}
