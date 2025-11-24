using Microsoft.ML.Data;

namespace Inspira.Trainer.Models
{
    public class RecommendationInput
    {
        [ColumnName("Label")]
        public float Rating { get; set; }

        public string UsuarioId { get; set; } = null!;

        public string CategoriaId { get; set; } = null!;
    }

    public class RecommendationOutput
    {
        public float Score { get; set; }
    }
}
