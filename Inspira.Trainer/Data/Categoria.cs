using System.ComponentModel.DataAnnotations.Schema;

namespace Inspira.Trainer.Data
{
    [Table("Categorias")]
    public class Categoria
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
    }
}
