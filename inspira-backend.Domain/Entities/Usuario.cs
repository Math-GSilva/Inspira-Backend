using inspira_backend.Domain.Enums;
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
    /// Entidade que representa um usuário na tabela 'Usuarios'.
    /// </summary>
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(150)]
        public string NomeCompleto { get; set; }

        [Required]
        [MaxLength(50)]
        public string NomeUsuario { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string SenhaHash { get; set; }

        public string? Bio { get; set; }

        public string? UrlFotoPerfil { get; set; }

        [Required]
        public UserRole TipoUsuario { get; set; } = UserRole.Comum;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        // Propriedades de Navegação
        // Um artista pode ter várias obras publicadas.
        public virtual ICollection<ObraDeArte> ObrasPublicadas { get; set; } = new List<ObraDeArte>();

        // Um usuário pode ter várias curtidas.
        public virtual ICollection<Curtida> Curtidas { get; set; } = new List<Curtida>();

        // Um usuário pode fazer vários comentários.
        public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

        // Lista de usuários que ESTE usuário segue.
        public virtual ICollection<Seguidor> Seguindo { get; set; } = new List<Seguidor>();

        // Lista de usuários que seguem ESTE usuário.
        public virtual ICollection<Seguidor> Seguidores { get; set; } = new List<Seguidor>();
    }
}
