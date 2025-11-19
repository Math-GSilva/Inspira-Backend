using Microsoft.EntityFrameworkCore;

namespace Inspira.Trainer.Data
{
    public class TrainingDbContext : DbContext
    {
        public TrainingDbContext(DbContextOptions<TrainingDbContext> options) : base(options) { }

        public DbSet<Curtida> Curtidas { get; set; } = null!;
        public DbSet<ObraDeArte> ObrasDeArte { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<UsuarioPreferenciaCategoria> UsuarioPreferenciaCategorias { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Curtida>()
                .HasOne(c => c.ObraDeArte)
                .WithMany()
                .HasForeignKey(c => c.ObraDeArteId);

            modelBuilder.Entity<Curtida>()
                .HasKey(c => new { c.UsuarioId, c.ObraDeArteId });

            modelBuilder.Entity<UsuarioPreferenciaCategoria>()
                .HasKey(p => new { p.UsuarioId, p.CategoriaId });

            modelBuilder.Entity<Usuario>().HasKey(u => u.Id);
            modelBuilder.Entity<Categoria>().HasKey(c => c.Id);
        }
    }
}