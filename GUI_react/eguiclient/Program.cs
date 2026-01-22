using Microsoft.EntityFrameworkCore;
using CinemaTicketSystem.Data;
using CinemaTicketSystem.Services;
using CinemaTicketSystem.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Use SQLite for simplicity
builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseSqlite("Data Source=cinema.db"));

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<CinemaDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        
        logger.LogInformation("🎬 Creating database...");
        context.Database.EnsureCreated();
        logger.LogInformation("✅ Database created!");
        
        // Seed admin user
        if (!context.Users.Any(u => u.Email == "admin@cinema.com"))
        {
            var admin = new User
            {
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@cinema.com",
                PhoneNumber = "+1234567890",
                PasswordHash = passwordHasher.HashPassword("Admin@123"),
                IsAdmin = true,
                RowVersion = Array.Empty<byte>()
            };
            context.Users.Add(admin);
            context.SaveChanges();
            logger.LogInformation("👤 Admin created: admin@cinema.com / Admin@123");
        }
        
        // Seed test user
        if (!context.Users.Any(u => u.Email == "john@email.com"))
        {
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@email.com",
                PhoneNumber = "+1234567891",
                PasswordHash = passwordHasher.HashPassword("User@123"),
                IsAdmin = false,
                RowVersion = Array.Empty<byte>()
            };
            context.Users.Add(user);
            context.SaveChanges();
            logger.LogInformation("👤 User created: john@email.com / User@123");
        }
        
        // Seed sample screenings
        if (!context.Screenings.Any())
        {
            var cinema1 = context.Cinemas.FirstOrDefault(c => c.Name == "Cinema Grand");
            var cinema2 = context.Cinemas.FirstOrDefault(c => c.Name == "Studio Cozy");
            var cinema3 = context.Cinemas.FirstOrDefault(c => c.Name == "Mega Screen");
            
            if (cinema1 != null && cinema2 != null && cinema3 != null)
            {
                var now = DateTime.UtcNow;
                var screenings = new List<Screening>
                {
                    new Screening { CinemaId = cinema1.Id, MovieTitle = "The Matrix Resurrections", StartDateTime = now.AddDays(3) },
                    new Screening { CinemaId = cinema1.Id, MovieTitle = "Inception", StartDateTime = now.AddDays(4) },
                    new Screening { CinemaId = cinema2.Id, MovieTitle = "Dune: Part Two", StartDateTime = now.AddDays(3) },
                    new Screening { CinemaId = cinema2.Id, MovieTitle = "Interstellar", StartDateTime = now.AddDays(5) },
                    new Screening { CinemaId = cinema3.Id, MovieTitle = "Oppenheimer", StartDateTime = now.AddDays(3) },
                    new Screening { CinemaId = cinema3.Id, MovieTitle = "The Dark Knight", StartDateTime = now.AddDays(6) }
                };
                
                context.Screenings.AddRange(screenings);
                context.SaveChanges();
                logger.LogInformation("🎥 Sample screenings created!");
            }
        }
        
        logger.LogInformation("🚀 Application ready!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error during database initialization");
    }
}

app.Run();