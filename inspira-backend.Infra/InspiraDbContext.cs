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

        // Mapeamento das suas entidades para tabelas do banco de dados
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<ObraDeArte> ObrasDeArte { get; set; }
        public DbSet<Curtida> Curtidas { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Seguidor> Seguidores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----- Configurações de Chaves e Relacionamentos (Fluent API) -----

            // Configuração da chave primária composta para a tabela Curtidas
            modelBuilder.Entity<Curtida>()
                .HasKey(c => new { c.UsuarioId, c.ObraDeArteId });

            // Configuração da chave primária composta para a tabela Seguidores
            modelBuilder.Entity<Seguidor>()
                .HasKey(s => new { s.SeguidorId, s.SeguidoId });

            // Configuração do relacionamento N-para-N de Seguidores
            // Define o relacionamento: Um Seguidor (usuário) tem muitos "Seguindo"
            modelBuilder.Entity<Seguidor>()
                .HasOne(s => s.SeguidorUsuario)
                .WithMany(u => u.Seguindo)
                .HasForeignKey(s => s.SeguidorId)
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata ciclica

            // Define o relacionamento: Um Seguido (usuário) tem muitos "Seguidores"
            modelBuilder.Entity<Seguidor>()
                .HasOne(s => s.SeguidoUsuario)
                .WithMany(u => u.Seguidores)
                .HasForeignKey(s => s.SeguidoId)
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata ciclica

            // Mapeamento do Enum para o tipo 'user_role' do PostgreSQL
            // Isso garante que o EF Core crie o tipo ENUM no banco de dados
            modelBuilder.HasPostgresEnum<UserRole>();
        }
    }
}
