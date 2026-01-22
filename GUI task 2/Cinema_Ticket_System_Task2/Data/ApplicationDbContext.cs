using CinemaTicketSystem.Models;
using Microsoft.AspNetCore.Identity;
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
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Seat> Seats { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Booking entity
            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)");

            // Fix the relationship: Booking -> Screening (many-to-one)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Screening)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ScreeningId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Seat relationships
            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Booking)
                .WithMany(b => b.Seats)
                .HasForeignKey(s => s.BookingId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Seat>()
                .Property(s => s.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();

            // Seed Admin User
            var adminUser = new ApplicationUser
            {
                Id = "admin-user-id",
                UserName = "admin@cinema.com",
                NormalizedUserName = "ADMIN@CINEMA.COM",
                Email = "admin@cinema.com",
                NormalizedEmail = "ADMIN@CINEMA.COM",
                EmailConfirmed = true,
                SecurityStamp = "STATIC-SECURITY-STAMP-12345"
            };

            var passwordHasher = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");

            modelBuilder.Entity<ApplicationUser>().HasData(adminUser);

            // Seed Admin Role
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "admin-role-id",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                }
            );

            // Assign Admin Role to Admin User
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "admin-role-id",
                    UserId = "admin-user-id"
                }
            );

            // Seed Cinemas
            modelBuilder.Entity<Cinema>().HasData(
                new Cinema
                {
                    Id = 1,
                    Name = "Grand Cinema Hall",
                    Rows = 10,
                    SeatsPerRow = 15
                },
                new Cinema
                {
                    Id = 2,
                    Name = "Cozy Theater",
                    Rows = 8,
                    SeatsPerRow = 12
                },
                new Cinema
                {
                    Id = 3,
                    Name = "Premium Auditorium",
                    Rows = 12,
                    SeatsPerRow = 20
                },
                new Cinema
                {
                    Id = 4,
                    Name = "Small Screening Room",
                    Rows = 6,
                    SeatsPerRow = 10
                }
            );
        }
    }
}
