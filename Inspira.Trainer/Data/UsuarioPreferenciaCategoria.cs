using System.ComponentModel.DataAnnotations.Schema;

namespace Inspira.Trainer.Data
{
    [Table("UsuarioPreferenciaCategoria")]
    public class UsuarioPreferenciaCategoria
    {
        public Guid UsuarioId { get; set; }
        public Guid CategoriaId { get; set; }
        public double Score { get; set; }
        public DateTime DataCalculo { get; set; } = DateTime.UtcNow;
    }
}
