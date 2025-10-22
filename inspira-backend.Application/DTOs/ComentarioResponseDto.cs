using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.DTOs
{
    public class ComentarioResponseDto
    {
        public Guid Id { get; set; }
        public string Conteudo { get; set; }
        public DateTime DataComentario { get; set; }
        public string? AutorUsername { get; set; }
        public Guid AutorId { get; set; }
        public string? UrlFotoPerfil { get; set; }
        public ICollection<ComentarioResponseDto> Respostas { get; set; } = new List<ComentarioResponseDto>();
    }
}
