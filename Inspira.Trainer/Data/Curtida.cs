using System.ComponentModel.DataAnnotations.Schema;

namespace Inspira.Trainer.Data
{
    [Table("Curtidas")]
    public class Curtida
    {
        public Guid UsuarioId { get; set; }
        public Guid ObraDeArteId { get; set; }
        public ObraDeArte ObraDeArte { get; set; } = null!;
    }
}