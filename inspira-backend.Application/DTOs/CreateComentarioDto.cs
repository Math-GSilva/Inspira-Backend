using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.DTOs
{
    public class CreateComentarioDto
    {
        [Required, MaxLength(1000)]
        public string Conteudo { get; set; }
    }
}
