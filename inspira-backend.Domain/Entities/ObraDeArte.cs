using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma publicação na tabela 'ObrasDeArte'.
    /// </summary>
    [Table("ObrasDeArte")]
    public class ObraDeArte
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataPublicacao { get; set; }

        public string? UrlMidia { get; set; }

        public byte[]? DadosMidia { get; set; }
        public string? TipoConteudoMidia { get; set; }

        public Guid UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public Guid CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
        public ICollection<Curtida> Curtidas { get; set; } = new List<Curtida>();
    }
}
