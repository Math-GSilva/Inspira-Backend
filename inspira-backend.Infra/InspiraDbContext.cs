using inspira_backend.Domain.Entities;
using inspira_backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace inspira_backend.Infra
{
    public class InspiraDbContext : DbContext
    {
        public InspiraDbContext(DbContextOptions<InspiraDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<ObraDeArte> ObrasDeArte { get; set; }
        public DbSet<Curtida> Curtidas { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Seguidor> Seguidores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Curtida>()
                .HasKey(c => new { c.UsuarioId, c.ObraDeArteId });

            modelBuilder.Entity<Seguidor>()
                .HasKey(s => new { s.SeguidorId, s.SeguidoId });

            modelBuilder.Entity<Seguidor>()
                .HasOne(s => s.SeguidorUsuario)
                .WithMany(u => u.Seguindo)
                .HasForeignKey(s => s.SeguidorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Seguidor>()
                .HasOne(s => s.SeguidoUsuario)
                .WithMany(u => u.Seguidores)
                .HasForeignKey(s => s.SeguidoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.CategoriaPrincipal)
                .WithMany(c => c.UsuariosComEstaCategoriaPrincipal)
                .HasForeignKey(u => u.CategoriaPrincipalId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.HasPostgresEnum<UserRole>();
        }
    }
}
