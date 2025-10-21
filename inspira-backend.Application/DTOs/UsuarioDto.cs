using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Application.DTOs
{
    public class UsuarioProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string NomeCompleto { get; set; }
        public string? Bio { get; set; }
        public string? UrlFotoPerfil { get; set; }
        public int ContagemSeguidores { get; set; }
        public int ContagemSeguindo { get; set; }
        public bool SeguidoPeloUsuarioAtual { get; set; }
    }
}
