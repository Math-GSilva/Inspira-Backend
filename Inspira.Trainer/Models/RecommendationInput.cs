using Microsoft.ML.Data;

namespace Inspira.Trainer.Models
{
    /// <summary>
    /// Define o formato dos dados de entrada para o ML.NET.
    /// Os nomes das propriedades (ex: "UsuarioId") devem corresponder
    /// exatamente aos nomes usados no pipeline de treinamento.
    /// </summary>
    public class RecommendationInput
    {
        [ColumnName("Label")]
        public float Rating { get; set; }

        public string UsuarioId { get; set; } = null!;

        public string CategoriaId { get; set; } = null!;
    }

    /// <summary>
    /// Define o formato da saída (a previsão)
    /// </summary>
    public class RecommendationOutput
    {
        public float Score { get; set; }
    }
}
