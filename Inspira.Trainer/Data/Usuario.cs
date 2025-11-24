using System.ComponentModel.DataAnnotations.Schema;

namespace Inspira.Trainer.Data
{
    [Table("Usuarios")]
    public class Usuario
    {
        public Guid Id { get; set; }
        public string? NomeUsuario { get; set; }
    }
}
