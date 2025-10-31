﻿using Microsoft.AspNetCore.Http;
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

        [MaxLength(500)]
        public string? Bio { get; set; }

        public IFormFile? FotoPerfil { get; set; }

        [Url(ErrorMessage = "A URL do Portfólio não é válida.")]
        public string? UrlPortifolio { get; set; }

        [Url(ErrorMessage = "A URL do LinkedIn não é válida.")]
        public string? UrlLinkedin { get; set; }

        [Url(ErrorMessage = "A URL do Instagram não é válida.")]
        public string? UrlInstagram { get; set; }
        public Guid? CategoriaPrincipalId { get; set; }
    }
}
