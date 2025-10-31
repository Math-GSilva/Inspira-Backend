using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inspira.Trainer.Data
{
    [Table("Usuarios")]
    public class Usuario
    {
        public Guid Id { get; set; }
        public string? NomeUsuario { get; set; }
    }
}
