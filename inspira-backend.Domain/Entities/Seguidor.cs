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
        // Chaves Estrangeiras que formam a Chave Primária Composta
        // Representa o usuário que está seguindo.
        [Required]
        public Guid SeguidorId { get; set; }
        public virtual Usuario SeguidorUsuario { get; set; }

        // Representa o usuário que está sendo seguido.
        [Required]
        public Guid SeguidoId { get; set; }
        public virtual Usuario SeguidoUsuario { get; set; }

        public DateTime DataSeguindo { get; set; } = DateTime.UtcNow;
    }
}
