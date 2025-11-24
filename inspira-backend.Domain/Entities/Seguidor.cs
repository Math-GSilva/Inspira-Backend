using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace inspira_backend.Domain.Entities
{
    [Table("Seguidores")]
    public class Seguidor
    {
        [Required]
        public Guid SeguidorId { get; set; }
        public virtual Usuario SeguidorUsuario { get; set; }

        [Required]
        public Guid SeguidoId { get; set; }
        public virtual Usuario SeguidoUsuario { get; set; }

        public DateTime DataSeguindo { get; set; } = DateTime.UtcNow;
    }
}
