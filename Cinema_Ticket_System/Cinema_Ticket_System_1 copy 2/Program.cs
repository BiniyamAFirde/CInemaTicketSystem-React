using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CinemaTicketSystem.Data;
using CinemaTicketSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0)),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)
    ));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Apply database initialization and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Starting database initialization...");
        
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Apply database initialization with retry logic
        int retryCount = 0;
        const int maxRetries = 10;
        const int delayMilliseconds = 5000;
        
        while (retryCount < maxRetries)
        {
            try
            {
                logger.LogInformation("Database initialization attempt {Attempt}/{MaxRetries}...", retryCount + 1, maxRetries);
                
                // Check if database exists and can be connected to
                var canConnect = context.Database.CanConnect();
                if (!canConnect)
                {
                    logger.LogWarning("Cannot connect to database. Retrying...");
                    throw new Exception("Database connection failed");
                }
                
                // Delete and recreate database to ensure clean state
                logger.LogInformation("Ensuring database is created...");
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
                
                logger.LogInformation("Database created successfully with all tables.");
                
                // Seed data after successful database creation
                await SeedData(context, userManager, roleManager, logger);
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    logger.LogError(ex, "Database initialization failed after {MaxRetries} attempts.", maxRetries);
                    throw new Exception($"Failed to initialize database after {maxRetries} attempts. Error: {ex.Message}", ex);
                }
                
                logger.LogWarning("Database initialization attempt {Attempt} failed: {Error}. Retrying in {Delay} seconds...", 
                    retryCount, ex.Message, delayMilliseconds / 1000);
                Thread.Sleep(delayMilliseconds);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Critical error occurred during database initialization.");
        throw; // Throw to prevent app from starting with broken database
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

async Task SeedData(ApplicationDbContext context, UserManager<ApplicationUser> userManager, 
    RoleManager<IdentityRole> roleManager, ILogger<Program> logger)
{
    try
    {
        logger.LogInformation("Starting database seeding...");
        
        // Create roles
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            logger.LogInformation("Admin role created.");
        }

        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
            logger.LogInformation("User role created.");
        }

        // Create admin user
        var adminEmail = "admin@cinema.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                PhoneNumber = "+1234567890",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Admin user created successfully. Email: {Email}", adminEmail);
            }
            else
            {
                logger.LogWarning("Failed to create admin user. Errors: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Sample movies
        if (!context.Movies.Any())
        {
            var movies = new List<Movie>
            {
                new Movie
                {
                    Title = "One of Them Days",
                    Description = "A thrilling sci-fi adventure about time travel and parallel universes.",
                    Genre = "Science Fiction",
                    DurationMinutes = 148,
                    ReleaseDate = new DateTime(2024, 11, 1)
                },
                new Movie
                {
                    Title = "Kill the Jockey",
                    Description = "A romantic drama set in the bustling streets of New York.",
                    Genre = "Romance",
                    DurationMinutes = 125,
                    ReleaseDate = new DateTime(2024, 10, 15)
                },
                new Movie
                {
                    Title = "The Mastermind",
                    Description = "A gripping mystery where a retired detective must solve one last case.",
                    Genre = "Thriller",
                    DurationMinutes = 130,
                    ReleaseDate = new DateTime(2024, 11, 8)
                },
                new Movie
                {
                    Title = "Sinners",
                    Description = "A historical epic about the rise and fall of an empire.",
                    Genre = "History",
                    DurationMinutes = 180,
                    ReleaseDate = new DateTime(2024, 9, 20)
                },
                new Movie
                {
                    Title = "Roofman",
                    Description = "A heartwarming animated tale for the whole family.",
                    Genre = "Animation",
                    DurationMinutes = 95,
                    ReleaseDate = new DateTime(2024, 12, 25)
                },
                new Movie
                {
                    Title = "Peter Hujar's Day",
                    Description = "A mind-bending psychological thriller.",
                    Genre = "Thriller",
                    DurationMinutes = 118,
                    ReleaseDate = new DateTime(2024, 10, 5)
                },
                new Movie
                {
                    Title = "Sentimental Value",
                    Description = "An inspiring story of a musician's journey to fame.",
                    Genre = "Musical",
                    DurationMinutes = 140,
                    ReleaseDate = new DateTime(2024, 11, 22)
                }
            };

            context.Movies.AddRange(movies);
            await context.SaveChangesAsync();
            logger.LogInformation("Sample movies seeded successfully.");
        }
        
        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Seeding error: {Message}", ex.Message);
        throw;
    }
}