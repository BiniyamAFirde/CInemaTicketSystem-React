#!/bin/bash

# Create UserManagementController.cs
cat > Controllers/UserManagementController.cs << 'CONTROLLER'
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
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Update user properties
            user.FirstName = model.FirstName ?? string.Empty;
            user.LastName = model.LastName ?? string.Empty;
            user.PhoneNumber = model.PhoneNumber ?? string.Empty;
            user.DateOfBirth = model.DateOfBirth;

            // Update user using UserManager
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
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

                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction("Index");
            }

            // If update failed, add errors to ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // Reload available roles
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

            // Prevent deleting yourself
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
                    await _userManager.AddToRoleAsync(user, model.Role);
                    TempData["SuccessMessage"] = "User added successfully!";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}
CONTROLLER

# Create ProfileEditViewModel.cs
cat > ViewModels/ProfileEditViewModel.cs << 'VIEWMODEL'
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaTicketSystem.ViewModels
{
    public class ProfileEditViewModel
    {
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        public List<string> Roles { get; set; } = new List<string>();

        public List<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
    }
}
VIEWMODEL

# Create Edit.cshtml
cat > Views/UserManagement/Edit.cshtml << 'VIEW'
@model CinemaTicketSystem.ViewModels.ProfileEditViewModel
@{
    ViewData["Title"] = "Edit User";
}

<div class="row justify-content-center">
    <div class="col-md-6">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white">
                <h4 class="mb-0">Edit User</h4>
            </div>
            <div class="card-body p-4">
                <form method="post" novalidate>
                    <input type="hidden" asp-for="Id" />
                    
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger" role="alert"></div>
                    
                    <div class="mb-3">
                        <label asp-for="FirstName" class="form-label"></label>
                        <input asp-for="FirstName" class="form-control" required />
                        <span asp-validation-for="FirstName" class="text-danger small"></span>
                    </div>
                    
                    <div class="mb-3">
                        <label asp-for="LastName" class="form-label"></label>
                        <input asp-for="LastName" class="form-control" required />
                        <span asp-validation-for="LastName" class="text-danger small"></span>
                    </div>
                    
                    <div class="mb-3">
                        <label asp-for="Email" class="form-label"></label>
                        <input asp-for="Email" type="email" class="form-control" disabled />
                        <small class="text-muted">Email cannot be changed</small>
                    </div>
                    
                    <div class="mb-3">
                        <label asp-for="PhoneNumber" class="form-label"></label>
                        <input asp-for="PhoneNumber" type="tel" class="form-control" />
                        <span asp-validation-for="PhoneNumber" class="text-danger small"></span>
                    </div>
                    
                    <div class="mb-3">
                        <label asp-for="DateOfBirth" class="form-label"></label>
                        <input asp-for="DateOfBirth" type="date" class="form-control" />
                        <span asp-validation-for="DateOfBirth" class="text-danger small"></span>
                    </div>
                    
                    <div class="mb-3">
                        <label class="form-label">User Roles</label>
                        @foreach (var role in Model.AvailableRoles)
                        {
                            <div class="form-check">
                                <input type="checkbox" 
                                       class="form-check-input" 
                                       name="Roles" 
                                       value="@role.Value" 
                                       id="role_@role.Value"
                                       @(Model.Roles.Contains(role.Value ?? "") ? "checked" : "") />
                                <label class="form-check-label" for="role_@role.Value">
                                    @role.Text
                                </label>
                            </div>
                        }
                        <span asp-validation-for="Roles" class="text-danger small"></span>
                    </div>
                    
                    <button type="submit" class="btn btn-primary w-100">Save Changes</button>
                    <a asp-action="Index" class="btn btn-secondary w-100 mt-2">Cancel</a>
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
VIEW

echo "Files created successfully!"
