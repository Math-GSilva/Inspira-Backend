using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
