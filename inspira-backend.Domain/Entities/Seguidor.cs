using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Domain.Entities
{
    /// <summary>
    /// Entidade de associação para a relação de seguidores (N-para-N entre Usuários).
    /// Representa a tabela 'Seguidores'.
    /// </summary>
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
