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
                
                var strategy = _context.Database.CreateExecutionStrategy();

                try
                {
                    await strategy.ExecuteAsync(async () =>
                    {
                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                var result = await _userManager.CreateAsync(user, model.Password);
                                
                                if (!result.Succeeded)
                                {
                                    throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
                                }

                                await transaction.CommitAsync();

                                // Optionally sign in the user automatically after registration
                                await _signInManager.SignInAsync(user, isPersistent: false);
                                return RedirectToAction("Index", "Home");
                            }
                            catch (Exception)
                            {
                                await transaction.RollbackAsync();
                                throw;
                            }
                        }
                    });
                }
                catch (InvalidOperationException ex)
                {
                    foreach (var error in ex.Message.Split(", "))
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return View(model);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred during registration.");
                    return View(model);
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
                // 🔄 ATOMIC: Store the ConcurrencyStamp as RowVersion for optimistic concurrency control
                RowVersion = string.IsNullOrEmpty(user.ConcurrencyStamp) 
                    ? null 
                    : System.Text.Encoding.UTF8.GetBytes(user.ConcurrencyStamp)
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

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // 🔄 ATOMIC OPERATION: Check concurrency using ConcurrencyStamp
            if (model.RowVersion != null)
            {
                var currentRowVersion = string.IsNullOrEmpty(user.ConcurrencyStamp) 
                    ? null 
                    : System.Text.Encoding.UTF8.GetBytes(user.ConcurrencyStamp);
                    
                if (!AreByteArraysEqual(model.RowVersion, currentRowVersion))
                {
                    TempData["ErrorMessage"] = "Your profile was modified by another session. Please refresh and try again.";
                    return RedirectToAction("Profile");
                }
            }

            user.FirstName = model.FirstName ?? string.Empty;
            user.LastName = model.LastName ?? string.Empty;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;

            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // 🔄 ATOMIC: Set original ConcurrencyStamp for optimistic concurrency
                            if (model.RowVersion != null && !string.IsNullOrEmpty(user.ConcurrencyStamp))
                            {
                                _context.Entry(user).Property(u => u.ConcurrencyStamp)
                                    .OriginalValue = System.Text.Encoding.UTF8.GetString(model.RowVersion);
                            }

                            var result = await _userManager.UpdateAsync(user);

                            if (!result.Succeeded)
                            {
                                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
                            }

                            await transaction.CommitAsync();

                            // Refresh the security principal to reflect changes
                            await _signInManager.RefreshSignInAsync(user);
                            TempData["SuccessMessage"] = "Profile updated successfully!";
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                });

                return RedirectToAction("Profile");
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Your profile was modified by another session. Please refresh and try again.";
                return RedirectToAction("Profile");
            }
            catch (InvalidOperationException ex)
            {
                foreach (var error in ex.Message.Split(", "))
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating your profile.");
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

        // Helper method to compare byte arrays for concurrency check
        private bool AreByteArraysEqual(byte[]? array1, byte[]? array2)
        {
            if (array1 == null && array2 == null) return true;
            if (array1 == null || array2 == null) return false;
            if (array1.Length != array2.Length) return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            }

            return true;
        }
    }
}