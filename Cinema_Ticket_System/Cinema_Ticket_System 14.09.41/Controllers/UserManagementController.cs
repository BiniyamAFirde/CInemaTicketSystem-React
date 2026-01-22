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
        private readonly ApplicationDbContext _context;

        public UserManagementController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
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

            var model = new ProfileEditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                DateOfBirth = user.DateOfBirth ?? DateTime.Now,
                Version = user.Version
            };

            return View(model);
        }

        // POST: UserManagement/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Entry(user).Property("Version").OriginalValue = model.Version;

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;

            try
            {
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // If updating the current user, refresh their session
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser?.Id == user.Id)
                    {
                        await _userManager.UpdateSecurityStampAsync(user);
                    }
                    TempData["SuccessMessage"] = "User updated successfully!";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync();
                if (databaseValues == null)
                {
                    ModelState.AddModelError(string.Empty, "The user was deleted by another user.");
                }
                else
                {
                    var databaseUser = (ApplicationUser)databaseValues.ToObject();
                    ModelState.AddModelError(string.Empty, "The user you are trying to edit has been modified by another user. " +
                                                            "Your changes have been cancelled. Please review the current values and try again.");
                    model.FirstName = databaseUser.FirstName;
                    model.LastName = databaseUser.LastName;
                    model.PhoneNumber = databaseUser.PhoneNumber;
                    model.DateOfBirth = databaseUser.DateOfBirth ?? DateTime.Now;
                    model.Version = databaseUser.Version;
                }
            }

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
        public async Task<IActionResult> DeleteConfirmed(string id, byte[] version)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            // Prevent deleting the last admin or yourself
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account!";
                return RedirectToAction("Index");
            }

            _context.Entry(user).Property("Version").OriginalValue = version;

            try
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "User deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete user.";
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "The user has been modified by another user. Please refresh and try again.";
            }

            return RedirectToAction("Index");
        }
    }
}
