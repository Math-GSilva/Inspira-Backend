using System.ComponentModel.DataAnnotations;

namespace inspira_backend.Application.DTOs
{
    public class UpdateObraDeArteDto
    {
        [Required, MaxLength(200)]
        public string Titulo { get; set; }

        [Required]
        public string Descricao { get; set; }
    }
}
