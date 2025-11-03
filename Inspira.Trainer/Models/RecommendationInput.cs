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
        [ColumnName("Label")] // O "Label" é a nota (o que queremos prever/aprender)
        public float Rating { get; set; }

        public string UsuarioId { get; set; } = null!; // Mudamos de uint para string

        public string CategoriaId { get; set; } = null!; // Mudamos de uint para string
    }

    /// <summary>
    /// Define o formato da saída (a previsão)
    /// </summary>
    public class RecommendationOutput
    {
        // O ML.NET coloca a previsão de nota/score aqui
        public float Score { get; set; }
    }
}
