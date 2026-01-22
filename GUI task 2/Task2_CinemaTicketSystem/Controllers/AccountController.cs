using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CinemaTicketSystem.Models;
using CinemaTicketSystem.ViewModels;
using CinemaTicketSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth
                };
                
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // Add user to default "User" role
                    await _userManager.AddToRoleAsync(user, "User");
                    
                    // Optionally sign in the user automatically after registration
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find user by email
                var user = await _userManager.FindByEmailAsync(model.Email);
                
                if (user != null && user.UserName != null)
                {
                    // Sign in using username (not email)
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                    
                    if (result.Succeeded)
                    {
                        // Check if user is admin
                        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                        if (isAdmin)
                        {
                            return RedirectToAction("Index", "Screening");
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }
                
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                RowVersion = user.RowVersion  // ✅ Changed from Version to RowVersion
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return NotFound();
            }

            // ✅ Changed from "Version" to using strongly-typed RowVersion
            _context.Entry(user).Property(u => u.RowVersion).OriginalValue = model.RowVersion;

            user.FirstName = model.FirstName ?? string.Empty;
            user.LastName = model.LastName ?? string.Empty;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;

            try
            {
                await _context.SaveChangesAsync();
                await _signInManager.RefreshSignInAsync(user);
                
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync();

                if (databaseValues == null)
                {
                    ModelState.AddModelError(string.Empty, "Your profile was deleted by an administrator.");
                }
                else
                {
                    var databaseUser = (ApplicationUser)databaseValues.ToObject();
                    ModelState.AddModelError(string.Empty, "Your profile has been modified by another user. Your changes were not applied. Please review the updated values and save again.");
                    
                    model.FirstName = databaseUser.FirstName ?? string.Empty;
                    model.LastName = databaseUser.LastName ?? string.Empty;
                    model.PhoneNumber = databaseUser.PhoneNumber;
                    model.DateOfBirth = databaseUser.DateOfBirth;
                    model.RowVersion = databaseUser.RowVersion;  // ✅ Changed from Version to RowVersion
                }
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Could not save your profile.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Bookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var bookings = await _context.Bookings
                .Where(b => b.UserId == user.Id)
                .Include(b => b.Screening)
                .ThenInclude(s => s!.Movie)
                .Include(b => b.Seats)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }
    }
}