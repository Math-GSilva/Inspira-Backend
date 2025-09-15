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
    /// Entidade para os comentários na tabela 'Comentarios'.
    /// </summary>
    [Table("Comentarios")]
    public class Comentario
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Conteudo { get; set; }

        public DateTime DataComentario { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }

        [Required]
        public Guid ObraDeArteId { get; set; }
        [ForeignKey("ObraDeArteId")]
        public virtual ObraDeArte ObraDeArte { get; set; }
        public Guid? ComentarioPaiId { get; set; }
        [ForeignKey("ComentarioPaiId")]
        public virtual Comentario? ComentarioPai { get; set; }

        public virtual ICollection<Comentario> Respostas { get; set; } = new List<Comentario>();
    }
}
