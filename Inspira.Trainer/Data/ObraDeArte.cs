using System.ComponentModel.DataAnnotations.Schema;

namespace Inspira.Trainer.Data
{
    [Table("ObrasDeArte")]
    public class ObraDeArte
    {
        public Guid Id { get; set; }
        public Guid CategoriaId { get; set; }
    }
}