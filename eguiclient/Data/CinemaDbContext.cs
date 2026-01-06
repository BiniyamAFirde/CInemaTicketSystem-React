// Data/CinemaDbContext.cs - FIXED FOR SQLITE CONCURRENCY
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
            
            // ✅ User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                
                // SQLite-compatible concurrency token
                entity.Property(e => e.RowVersion)
                    .IsConcurrencyToken()
                    .HasMaxLength(50);
            });
            
            // ✅ Cinema configuration
            modelBuilder.Entity<Cinema>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
            
            // ✅ Screening configuration
            modelBuilder.Entity<Screening>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Cinema)
                    .WithMany(c => c.Screenings)
                    .HasForeignKey(e => e.CinemaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // ✅ RESERVATION CONFIGURATION - KEY FOR CONCURRENCY HANDLING
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // ⭐ THIS IS THE CRITICAL PART FOR PREVENTING DOUBLE BOOKINGS
                // The UNIQUE index on (ScreeningId, Row, Seat) prevents two reservations
                // from being created for the same seat at the database level
                entity.HasIndex(e => new { e.ScreeningId, e.Row, e.Seat })
                    .IsUnique()
                    .HasDatabaseName("IX_Reservation_UniqueScreeningSeat");
                
                // ⚠️ REMOVED .IsRowVersion() - doesn't work in SQLite
                // The UNIQUE constraint above handles concurrency
                entity.Property(e => e.RowVersion)
                    .IsConcurrencyToken()
                    .IsRequired(false);
                
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
        
        // ✅ Override SaveChanges to update RowVersion
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                // Update timestamps for new reservations
                if (entry.Entity is Reservation reservation && entry.State == EntityState.Added)
                {
                    reservation.ReservedAt = DateTime.UtcNow;
                }
                
                // Generate new RowVersion for User entities when modified
                if (entry.Entity is User user)
                {
                    user.RowVersion = Guid.NewGuid().ToString();
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}