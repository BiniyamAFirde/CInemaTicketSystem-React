
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CinemaTicketSystem.Data;
using CinemaTicketSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure database connection
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=cinema.db"));

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

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    try
    {
        // Apply migrations
        context.Database.Migrate();
        
        // Only seed if database is ready
        await SeedData(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

async Task SeedData(ApplicationDbContext context, UserManager<ApplicationUser> userManager, 
    RoleManager<IdentityRole> roleManager)
{
    try
    {
        // Create roles
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
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
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seeding error: {ex.Message}");
    }
}