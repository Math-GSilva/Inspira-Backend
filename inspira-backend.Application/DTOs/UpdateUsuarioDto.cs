using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.DTOs
{
    public class UpdateUsuarioDto
    {
        [Required]
        [MaxLength(200)]
        public string NomeCompleto { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        [Required]
        public IFormFile? FotoPerfil { get; set; }
    }
}
