using CinemaTicketSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Screening> Screenings { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================================
            // 🔄 ATOMIC OPERATION CONFIGURATION
            // ============================================================
            // These configurations enable optimistic concurrency control
            // using RowVersion (timestamp) columns for atomic operations
            // ============================================================

            // Seat Entity Configuration
            modelBuilder.Entity<Seat>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ScreeningId)
                    .IsRequired();

                entity.Property(e => e.Row)
                    .IsRequired();

                entity.Property(e => e.SeatNumber)
                    .HasMaxLength(2)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasDefaultValue(SeatStatus.Available);

                // 🔄 ATOMIC: Configure RowVersion as concurrency token for MySQL
                // MySQL doesn't support SQL Server's rowversion, so we use timestamp with specific configuration
                entity.Property(e => e.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("timestamp(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");

                entity.HasIndex(e => new { e.ScreeningId, e.Row, e.SeatNumber })
                    .IsUnique()
                    .HasDatabaseName("IX_Unique_Seat");

                entity.HasOne(e => e.Screening)
                    .WithMany(s => s.Seats)
                    .HasForeignKey(e => e.ScreeningId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Booking)
                    .WithMany(b => b.Seats)
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Booking Entity Configuration
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.ScreeningId)
                    .IsRequired();

                entity.Property(e => e.BookingDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("Pending");

                entity.Property(e => e.TotalPrice)
                    .HasPrecision(10, 2)
                    .IsRequired();

                // 🔄 ATOMIC: Configure RowVersion as concurrency token for MySQL
                entity.Property(e => e.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("timestamp(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");

                entity.HasIndex(e => new { e.UserId, e.BookingDate })
                    .HasDatabaseName("IX_User_BookingDate");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Screening)
                    .WithMany(s => s.Bookings)
                    .HasForeignKey(e => e.ScreeningId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Screening Entity Configuration
            modelBuilder.Entity<Screening>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.MovieId)
                    .IsRequired();

                entity.Property(e => e.CinemaId)
                    .IsRequired();

                entity.Property(e => e.ScreeningDateTime)
                    .IsRequired();

                entity.Property(e => e.TicketPrice)
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("Active");

                entity.HasIndex(e => new { e.CinemaId, e.ScreeningDateTime })
                    .HasDatabaseName("IX_Cinema_ScreeningTime");

                entity.HasOne(e => e.Movie)
                    .WithMany(m => m.Screenings)
                    .HasForeignKey(e => e.MovieId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Cinema)
                    .WithMany(c => c.Screenings)
                    .HasForeignKey(e => e.CinemaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Movie Entity Configuration
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .HasMaxLength(300)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(2000);

                entity.Property(e => e.Genre)
                    .HasMaxLength(100);

                entity.Property(e => e.Director)
                    .HasMaxLength(100);

                entity.Property(e => e.Rating)
                    .HasPrecision(3, 1);

                entity.HasIndex(e => e.Title)
                    .HasDatabaseName("IX_Movie_Title");
            });

            // Cinema Entity Configuration
            modelBuilder.Entity<Cinema>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Location)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.Rows)
                    .IsRequired();

                entity.Property(e => e.SeatsPerRow)
                    .IsRequired();

                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("IX_Cinema_Name");
            });

            // ApplicationUser Configuration
            // 🔄 ATOMIC: ConcurrencyStamp is already configured in IdentityDbContext
            // It's automatically used for optimistic concurrency control
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName)
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .HasMaxLength(100);
            });
        }
    }
}