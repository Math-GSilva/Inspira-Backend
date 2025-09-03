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
    /// Entidade que representa uma publicação na tabela 'ObrasDeArte'.
    /// </summary>
    [Table("ObrasDeArte")]
    public class ObraDeArte
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; }

        public string? Descricao { get; set; }

        [Required]
        public string UrlMidia { get; set; }

        public DateTime DataPublicacao { get; set; } = DateTime.UtcNow;

        public bool Visivel { get; set; } = true;

        // Chaves Estrangeiras e Propriedades de Navegação
        [Required]
        public Guid ArtistaId { get; set; }
        [ForeignKey("ArtistaId")]
        public virtual Usuario Artista { get; set; }

        [Required]
        public int CategoriaId { get; set; }
        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; }

        // Uma obra pode ter várias curtidas.
        public virtual ICollection<Curtida> Curtidas { get; set; } = new List<Curtida>();

        // Uma obra pode ter vários comentários.
        public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    }
}
