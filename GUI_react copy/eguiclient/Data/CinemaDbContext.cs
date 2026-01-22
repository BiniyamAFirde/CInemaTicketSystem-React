using Microsoft.EntityFrameworkCore;
using CinemaTicketSystem.Models;
namespace CinemaTicketSystem.Data
{
    public class CinemaDbContext : DbContext
    {
        public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Screening> Screenings { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.RowVersion)
                    .IsConcurrencyToken()
                    .HasDefaultValue(new byte[] { });  // ← Add default value for SQLite
            });
            // Cinema configuration
            modelBuilder.Entity<Cinema>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
            // Screening configuration
            modelBuilder.Entity<Screening>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Cinema)
                    .WithMany(c => c.Screenings)
                    .HasForeignKey(e => e.CinemaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // Reservation configuration with concurrency control
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ScreeningId, e.Row, e.Seat }).IsUnique();
                entity.Property(e => e.RowVersion)
                    .IsConcurrencyToken()
                    .HasDefaultValue(new byte[] { });  // ← Add default value for SQLite
                
                entity.HasOne(e => e.Screening)
                    .WithMany(s => s.Reservations)
                    .HasForeignKey(e => e.ScreeningId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Reservations)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // Seed initial data
            SeedData(modelBuilder);
        }
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Cinemas
            modelBuilder.Entity<Cinema>().HasData(
                new Cinema { Id = 1, Name = "Cinema Grand", Rows = 10, SeatsPerRow = 15 },
                new Cinema { Id = 2, Name = "Studio Cozy", Rows = 5, SeatsPerRow = 8 },
                new Cinema { Id = 3, Name = "Mega Screen", Rows = 12, SeatsPerRow = 20 }
            );
        }
    }
}