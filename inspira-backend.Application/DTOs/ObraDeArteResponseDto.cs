using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.DTOs
{
    public class ObraDeArteResponseDto
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataPublicacao { get; set; }
        public string AutorUsername { get; set; }
        public string CategoriaNome { get; set; }
        public string? Url { get; set; }
        public int TotalCurtidas { get; set; }
    }
}
