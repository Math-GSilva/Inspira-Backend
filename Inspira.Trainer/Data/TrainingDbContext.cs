using Microsoft.EntityFrameworkCore;

namespace Inspira.Trainer.Data
{
    public class TrainingDbContext : DbContext
    {
        public TrainingDbContext(DbContextOptions<TrainingDbContext> options) : base(options) { }

        public DbSet<Curtida> Curtidas { get; set; } = null!;
        public DbSet<ObraDeArte> ObrasDeArte { get; set; } = null!;

        // --- ADICIONADO ---
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<UsuarioPreferenciaCategoria> UsuarioPreferenciaCategorias { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configura a relação (você pode ter que ajustar isso)
            modelBuilder.Entity<Curtida>()
                .HasOne(c => c.ObraDeArte)
                .WithMany() // Assumindo que ObraDeArte não tem uma lista de Curtidas
                .HasForeignKey(c => c.ObraDeArteId);

            // Chave primária composta para Curtidas (provavelmente)
            modelBuilder.Entity<Curtida>()
                .HasKey(c => new { c.UsuarioId, c.ObraDeArteId });

            // --- ADICIONADO ---
            // Define a Chave Primária Composta para a nova tabela de scores
            modelBuilder.Entity<UsuarioPreferenciaCategoria>()
                .HasKey(p => new { p.UsuarioId, p.CategoriaId });

            // Define as entidades mínimas (sem configuração de relação)
            modelBuilder.Entity<Usuario>().HasKey(u => u.Id);
            modelBuilder.Entity<Categoria>().HasKey(c => c.Id);
        }
    }
}