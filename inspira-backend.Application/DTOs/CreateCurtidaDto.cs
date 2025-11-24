using System.ComponentModel.DataAnnotations;

namespace inspira_backend.Application.DTOs
{
    public class CreateCurtidaDto
    {
        [Required]
        public Guid ObraDeArteId { get; set; }
    }
}
