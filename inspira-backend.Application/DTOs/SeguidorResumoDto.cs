using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.DTOs
{
    public class SeguidorResumoDto
    {
        public Guid UsuarioId { get; set; }
        public string Username { get; set; }
        public string NomeCompleto { get; set; }
        public string? UrlFotoPerfil { get; set; }
    }
}
