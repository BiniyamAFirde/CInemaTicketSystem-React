using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaTicketSystem.Models;
using CinemaTicketSystem.ViewModels;
using CinemaTicketSystem.Data;

namespace CinemaTicketSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: UserManagement/Index
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // GET: UserManagement/Edit/id
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var model = new ProfileEditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                DateOfBirth = user.DateOfBirth ?? DateTime.Now,
                RowVersion = user.RowVersion,
                Roles = userRoles.ToList(),
                AvailableRoles = allRoles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList()
            };

            return View(model);
        }

        // POST: UserManagement/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ProfileEditViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "User ID mismatch.";
                return NotFound();
            }

            ModelState.Remove("Email");

            if (!ModelState.IsValid)
            {
                var allRoles = await _roleManager.Roles.ToListAsync();
                model.AvailableRoles = allRoles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList();
                
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Set the original RowVersion for concurrency check
            _context.Entry(user).Property(u => u.RowVersion).OriginalValue = model.RowVersion;

            // Update user properties
            user.FirstName = model.FirstName ?? string.Empty;
            user.LastName = model.LastName ?? string.Empty;
            user.PhoneNumber = model.PhoneNumber ?? string.Empty;
            user.DateOfBirth = model.DateOfBirth;

            try
            {
                await _context.SaveChangesAsync();

                // Update roles after successful user update
                var userRoles = await _userManager.GetRolesAsync(user);
                var newRoles = model.Roles ?? new List<string>();

                var rolesToAdd = newRoles.Except(userRoles).ToList();
                var rolesToRemove = userRoles.Except(newRoles).ToList();

                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }

                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }

                // If updating the current user, refresh their session
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.Id == user.Id)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                }

                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync();

                if (databaseValues == null)
                {
                    ModelState.AddModelError(string.Empty, "The user was deleted by another admin.");
                }
                else
                {
                    var databaseUser = (ApplicationUser)databaseValues.ToObject();
                    ModelState.AddModelError(string.Empty, 
                        "The user has been modified by another admin. The current values have been loaded. Please review and try again.");
                    
                    // Reload current database values
                    model.FirstName = databaseUser.FirstName ?? string.Empty;
                    model.LastName = databaseUser.LastName ?? string.Empty;
                    model.PhoneNumber = databaseUser.PhoneNumber ?? string.Empty;
                    model.DateOfBirth = databaseUser.DateOfBirth ?? DateTime.Now;
                    model.RowVersion = databaseUser.RowVersion;
                }

                // Reload available roles
                var allRoles = await _roleManager.Roles.ToListAsync();
                model.AvailableRoles = allRoles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Could not save changes.");
                
                var allRoles = await _roleManager.Roles.ToListAsync();
                model.AvailableRoles = allRoles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList();

                return View(model);
            }
        }

        // GET: UserManagement/Delete/id
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: UserManagement/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id, byte[]? rowVersion)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account!";
                return RedirectToAction("Index");
            }

            // Set original RowVersion for concurrency check
            _context.Entry(user).Property(u => u.RowVersion).OriginalValue = rowVersion;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get all bookings for the user
                var bookings = await _context.Bookings
                    .Where(b => b.UserId == id)
                    .Include(b => b.Seats)
                    .ToListAsync();

                // Release seats and remove bookings
                foreach (var booking in bookings)
                {
                    foreach (var seat in booking.Seats)
                    {
                        seat.Status = SeatStatus.Available;
                    }
                    _context.Bookings.Remove(booking);
                }
                
                // Delete the user
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    await transaction.CommitAsync();
                    TempData["SuccessMessage"] = "User deleted successfully!";
                }
                else
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Failed to delete user. " + string.Join(" ", result.Errors.Select(e => e.Description));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "The user has been modified by another admin. Please refresh and try again.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "An error occurred while deleting the user.";
            }

            return RedirectToAction("Index");
        }

        // GET: UserManagement/AddUser
        [HttpGet]
        public async Task<IActionResult> AddUser()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var model = new RegisterViewModel
            {
                AvailableRoles = roles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList()
            };
            return View(model);
        }

        // POST: UserManagement/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }
                    TempData["SuccessMessage"] = "User added successfully!";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Reload roles if validation fails
            var roles = await _roleManager.Roles.ToListAsync();
            model.AvailableRoles = roles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = r.Name,
                Value = r.Name
            }).ToList();

            return View(model);
        }
    }
}