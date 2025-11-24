using inspira_backend.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace inspira_backend.Domain.Entities
{
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
        public string? UrlPortifolio { get; set; }
        public string? UrlLinkedin { get; set; }
        public string? UrlInstagram { get; set; }

        [Required]
        public UserRole TipoUsuario { get; set; } = UserRole.Comum;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
        public Guid? CategoriaPrincipalId { get; set; }
        public virtual Categoria? CategoriaPrincipal { get; set; }

        public virtual ICollection<ObraDeArte> ObrasPublicadas { get; set; } = new List<ObraDeArte>();

        public virtual ICollection<Curtida> Curtidas { get; set; } = new List<Curtida>();

        public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

        public virtual ICollection<Seguidor> Seguindo { get; set; } = new List<Seguidor>();

        public virtual ICollection<Seguidor> Seguidores { get; set; } = new List<Seguidor>();
    }
}
