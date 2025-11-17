using Microsoft.EntityFrameworkCore;
using WorkTimeTracker.Models;

namespace WorkTimeTracker.Data
{
    public class WorkTimeContext : DbContext
    {
        public WorkTimeContext(DbContextOptions<WorkTimeContext> options) : base(options)
        {
        }

        public DbSet<Pracownik> Pracownicy { get; set; } = null!;
        public DbSet<RejestrCzasu> Rejestry { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pracownik>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Imie).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Nazwisko).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Stanowisko).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.StawkaGodzinowa).HasPrecision(18,2);
            });

            modelBuilder.Entity<RejestrCzasu>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Data).IsRequired();
                entity.Property(e => e.LiczbaGodzin).HasPrecision(5,2);
                entity.Property(e => e.CzyUrlop).IsRequired();
                entity.Property(e => e.CzyNadgodziny).IsRequired();

                // Foreign key to Pracownik
                entity.HasOne<Pracownik>()
                      .WithMany()
                      .HasForeignKey("PracownikId")
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}