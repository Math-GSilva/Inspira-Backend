using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.DTOs
{
    public class CreateCurtidaDto
    {
        [Required]
        public Guid ObraDeArteId { get; set; }
    }
}
