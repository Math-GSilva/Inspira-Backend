using System.ComponentModel.DataAnnotations;

namespace inspira_backend.Application.DTOs
{
    public class CreateComentarioDto
    {
        [Required]
        public Guid ObraDeArteId { get; set; }

        [Required, MaxLength(1000)]
        public string Conteudo { get; set; }
    }
}
