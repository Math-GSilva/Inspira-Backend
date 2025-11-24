using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace inspira_backend.Application.DTOs
{
    public class CreateObraDeArteDto
    {
        [Required, MaxLength(200)]
        public string Titulo { get; set; }

        [Required]
        public string Descricao { get; set; }

        [Required]
        public IFormFile Midia { get; set; }

        [Required]
        public Guid CategoriaId { get; set; }
    }
}
