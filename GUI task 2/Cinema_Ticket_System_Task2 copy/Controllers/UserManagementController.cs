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

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
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
                RowVersion = user.Version,
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
                
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    TempData["ErrorMessage"] = error.ErrorMessage;
                }
                
                return View(model);
            }

            // BEGIN TRANSACTION - This ensures atomic operations for timestamp concurrency
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Fetch user within transaction to ensure fresh data
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.Id);

                    if (user == null)
                    {
                        await transaction.RollbackAsync();
                        return NotFound();
                    }

                    // CRITICAL: Set the original timestamp value for concurrency check
                    // This compares the RowVersion from the form with the current database value
                    _context.Entry(user).Property("Version").OriginalValue = model.RowVersion;

                    // Update user properties
                    user.FirstName = model.FirstName ?? string.Empty;
                    user.LastName = model.LastName ?? string.Empty;
                    user.PhoneNumber = model.PhoneNumber ?? string.Empty;
                    user.DateOfBirth = model.DateOfBirth;

                    // Save user profile changes - This will throw DbUpdateConcurrencyException
                    // if another admin modified the user (timestamp mismatch)
                    await _context.SaveChangesAsync();

                    // Update user roles
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var newRoles = model.Roles ?? new List<string>();

                    var rolesToAdd = newRoles.Except(userRoles).ToList();
                    var rolesToRemove = userRoles.Except(newRoles).ToList();

                    if (rolesToAdd.Any())
                    {
                        var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                        if (!addResult.Succeeded)
                        {
                            throw new Exception($"Failed to add roles: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                        }
                    }

                    if (rolesToRemove.Any())
                    {
                        var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                        if (!removeResult.Succeeded)
                        {
                            throw new Exception($"Failed to remove roles: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                        }
                    }

                    // Update security stamp if editing current user
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser?.Id == user.Id)
                    {
                        var stampResult = await _userManager.UpdateSecurityStampAsync(user);
                        if (!stampResult.Succeeded)
                        {
                            throw new Exception($"Failed to update security stamp: {string.Join(", ", stampResult.Errors.Select(e => e.Description))}");
                        }
                    }

                    // Commit all changes atomically - timestamp is automatically updated here
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "User updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // CRITICAL: This catch block handles the timestamp concurrency scenario
                    // When Admin 1 saves first, the timestamp changes in the database
                    // When Admin 2 tries to save, their old timestamp doesn't match
                    // This exception is thrown and ALL changes are rolled back (atomic)
                    await transaction.RollbackAsync();

                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    if (databaseValues == null)
                    {
                        TempData["ErrorMessage"] = "Unable to save changes. The user was deleted by another administrator.";
                        ModelState.AddModelError(string.Empty, "Unable to save changes. The user was deleted by another administrator.");
                    }
                    else
                    {
                        var databaseUser = (ApplicationUser)databaseValues.ToObject();
                        
                        // Show friendly error message to Admin 2
                        TempData["ErrorMessage"] = "The user you are editing was modified by another administrator. Your changes have not been saved. Please review the current values and try again.";
                        ModelState.AddModelError(string.Empty, "The user you are editing was modified by another administrator. Your changes have not been saved. The current values are displayed.");
                        
                        // Reload current database values into the form
                        model.FirstName = databaseUser.FirstName ?? string.Empty;
                        model.LastName = databaseUser.LastName ?? string.Empty;
                        model.PhoneNumber = databaseUser.PhoneNumber ?? string.Empty;
                        model.DateOfBirth = databaseUser.DateOfBirth ?? DateTime.Now;
                        model.RowVersion = databaseUser.Version; // Update with new timestamp
                        
                        // Reload current roles
                        var currentRoles = await _userManager.GetRolesAsync(databaseUser);
                        model.Roles = currentRoles.ToList();
                    }
                }
                catch (Exception ex)
                {
                    // Rollback transaction on any other error
                    await transaction.RollbackAsync();
                    
                    TempData["ErrorMessage"] = $"An error occurred while updating the user: {ex.Message}";
                    ModelState.AddModelError(string.Empty, $"An error occurred while updating the user: {ex.Message}");
                }
            }

            // Repopulate roles for the view (for re-display after error)
            var roles = await _roleManager.Roles.ToListAsync();
            model.AvailableRoles = roles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = r.Name,
                Value = r.Name
            }).ToList();

            return View(model);
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
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
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

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user.";
            }

            return RedirectToAction("Index");
        }

        // GET: UserManagement/AddUser
        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }

        // POST: UserManagement/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // BEGIN TRANSACTION - Make user creation and role assignment atomic
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
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

                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            await transaction.RollbackAsync();
                            return View(model);
                        }

                        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                        
                        if (!roleResult.Succeeded)
                        {
                            foreach (var error in roleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            await transaction.RollbackAsync();
                            return View(model);
                        }

                        // Commit all changes atomically
                        await transaction.CommitAsync();

                        TempData["SuccessMessage"] = "User added successfully!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, $"An error occurred while creating the user: {ex.Message}");
                    }
                }
            }

            return View(model);
        }
    }
}