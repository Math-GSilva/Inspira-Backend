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
    /// Entidade que representa uma categoria de arte na tabela 'Categorias'.
    /// </summary>
    [Table("Categorias")]
    public class Categoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Indica que o ID é auto-incrementável
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        public string? Descricao { get; set; }

        // Propriedade de Navegação
        // Uma categoria pode conter várias obras de arte.
        public virtual ICollection<ObraDeArte> ObrasDeArte { get; set; } = new List<ObraDeArte>();
    }
}
