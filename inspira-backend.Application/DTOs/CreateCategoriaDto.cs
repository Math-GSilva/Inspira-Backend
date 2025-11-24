using System.ComponentModel.DataAnnotations;

namespace inspira_backend.Application.DTOs
{
    public class CreateCategoriaDto
    {
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [MaxLength(500)]
        public string? Descricao { get; set; }
    }
}
