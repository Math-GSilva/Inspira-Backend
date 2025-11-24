using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace inspira_backend.Domain.Entities
{
    [Table("Categorias")]
    public class Categoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        public string? Descricao { get; set; }
        public virtual ICollection<ObraDeArte> ObrasDeArte { get; set; } = new List<ObraDeArte>();
        public virtual ICollection<Usuario> UsuariosComEstaCategoriaPrincipal { get; set; }
    }
}
