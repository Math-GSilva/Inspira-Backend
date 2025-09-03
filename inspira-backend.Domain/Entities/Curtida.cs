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
    /// Entidade de associação para curtidas, representando a tabela 'Curtidas'.
    /// Possui uma chave primária composta (UsuarioId, ObraDeArteId).
    /// </summary>
    [Table("Curtidas")]
    public class Curtida
    {
        // Chaves Estrangeiras que formam a Chave Primária Composta
        [Required]
        public Guid UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }

        [Required]
        public Guid ObraDeArteId { get; set; }
        [ForeignKey("ObraDeArteId")]
        public virtual ObraDeArte ObraDeArte { get; set; }

        public DateTime DataCurtida { get; set; } = DateTime.UtcNow;
    }
}
