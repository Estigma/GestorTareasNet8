using Microsoft.EntityFrameworkCore;
using TaskService.Models;

namespace TaskService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Tarea> Tareas { get; set; }        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tarea>(entity =>
            {
                entity.HasQueryFilter(t => t.Estado == true);
                entity.Property(t => t.EstadoTarea)
                    .HasConversion<string>()
                    .HasDefaultValue(EstadoTarea.Backlog);

                entity.ToTable(t => t.HasCheckConstraint("CK_Tarea_EstadoTarea", "[EstadoTarea] IN ('Backlog', 'Doing', 'InReview', 'Done')"));

                entity.Property(t => t.TiempoDesarrollo)
                    .HasDefaultValue(0);
            });
        }
    }
}