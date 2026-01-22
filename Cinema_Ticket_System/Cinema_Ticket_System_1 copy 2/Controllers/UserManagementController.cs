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
                // 🔄 ATOMIC: Store the ConcurrencyStamp as RowVersion for optimistic concurrency control
                RowVersion = string.IsNullOrEmpty(user.ConcurrencyStamp) ? null : System.Text.Encoding.UTF8.GetBytes(user.ConcurrencyStamp),
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
            // Ensure the ID matches
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "User ID mismatch.";
                return NotFound();
            }

            // Remove Email from validation since it's disabled and can't be changed
            ModelState.Remove("Email");

            if (!ModelState.IsValid)
            {
                // Reload available roles if validation fails
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

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == model.Id);
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
                    TempData["ErrorMessage"] = "This user was modified by another administrator. Please refresh and try again.";
                    return RedirectToAction("Edit", new { id = model.Id });
                }
            }

            // Update user properties
            user.FirstName = model.FirstName ?? string.Empty;
            user.LastName = model.LastName ?? string.Empty;
            user.PhoneNumber = model.PhoneNumber ?? string.Empty;
            user.DateOfBirth = model.DateOfBirth;

            // 🔄 ATOMIC OPERATION: Update user with optimistic concurrency control
            // UserManager automatically handles ConcurrencyStamp updates
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

                            // Update roles
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

                            await transaction.CommitAsync();
                            TempData["SuccessMessage"] = "User updated successfully!";
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

                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "The user was modified by another administrator. Please refresh and try again.";
                return RedirectToAction("Edit", new { id = model.Id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                
                // Reload available roles
                var roles = await _roleManager.Roles.ToListAsync();
                model.AvailableRoles = roles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the user.";
                
                // Reload available roles
                var roles = await _roleManager.Roles.ToListAsync();
                model.AvailableRoles = roles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
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
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            // Prevent deleting yourself
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account!";
                return RedirectToAction("Index");
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var result = await _userManager.DeleteAsync(user);

                            if (!result.Succeeded)
                            {
                                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
                            }

                            await transaction.CommitAsync();
                            TempData["SuccessMessage"] = "User deleted successfully!";
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                });
            }
            catch (Exception)
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
                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
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

                                await _userManager.AddToRoleAsync(user, model.Role);
                                await transaction.CommitAsync();

                                TempData["SuccessMessage"] = "User added successfully!";
                            }
                            catch (Exception)
                            {
                                await transaction.RollbackAsync();
                                throw;
                            }
                        }
                    });

                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the user.");
                }
            }

            return View(model);
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