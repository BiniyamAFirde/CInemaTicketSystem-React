// Controllers/UsersController.cs - COMPLETE REPLACEMENT
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaTicketSystem.Data;
using CinemaTicketSystem.DTOs;
using CinemaTicketSystem.Models;
using CinemaTicketSystem.Services;

namespace CinemaTicketSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CinemaDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            CinemaDbContext context, 
            IPasswordHasher passwordHasher,
            ILogger<UsersController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    IsAdmin = u.IsAdmin,
                    RowVersion = u.RowVersion // ✅ Now a string
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsAdmin = user.IsAdmin,
                RowVersion = user.RowVersion
            });
        }

        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser(UserCreateDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                IsAdmin = dto.IsAdmin,
                RowVersion = Guid.NewGuid().ToString() // ✅ Initialize
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsAdmin = user.IsAdmin,
                RowVersion = user.RowVersion
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto dto)
        {
            _logger.LogInformation($"🔄 Updating user {id} with RowVersion: {dto.RowVersion}");

            // ✅ CRITICAL: Use AsNoTracking to get fresh data from database
            var userInDb = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (userInDb == null)
            {
                _logger.LogWarning($"❌ User {id} not found");
                return NotFound(new { message = "User not found" });
            }

            _logger.LogInformation($"📊 Database RowVersion: {userInDb.RowVersion}");
            _logger.LogInformation($"📊 Client RowVersion: {dto.RowVersion}");

            // ✅ STEP 1: Check if RowVersions match (Optimistic Concurrency Check)
            if (userInDb.RowVersion != dto.RowVersion)
            {
                _logger.LogWarning($"⚠️ CONFLICT DETECTED for user {id}!");
                _logger.LogWarning($"   Database version: {userInDb.RowVersion}");
                _logger.LogWarning($"   Client version: {dto.RowVersion}");
                
                return Conflict(new 
                { 
                    message = "⚠️ CONFLICT: This user was modified by another admin. Please refresh the page to see the latest data.",
                    conflict = true,
                    latestData = new 
                    {
                        firstName = userInDb.FirstName,
                        lastName = userInDb.LastName,
                        phoneNumber = userInDb.PhoneNumber,
                        rowVersion = userInDb.RowVersion
                    }
                });
            }

            // ✅ STEP 2: Create a new tracked entity with updated values
            var user = new User
            {
                Id = id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = userInDb.Email, // Keep existing email
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = userInDb.PasswordHash, // Keep existing password
                IsAdmin = userInDb.IsAdmin, // Keep existing admin status
                RowVersion = dto.RowVersion // Set the original version
            };

            // ✅ STEP 3: Attach and mark as modified
            _context.Users.Attach(user);
            _context.Entry(user).State = EntityState.Modified;
            
            // Don't update these fields
            _context.Entry(user).Property(u => u.Email).IsModified = false;
            _context.Entry(user).Property(u => u.PasswordHash).IsModified = false;
            _context.Entry(user).Property(u => u.IsAdmin).IsModified = false;

            // ✅ STEP 4: Generate new RowVersion BEFORE saving
            var newRowVersion = Guid.NewGuid().ToString();
            user.RowVersion = newRowVersion;

            try
            {
                // ✅ STEP 5: Save changes (EF will check concurrency)
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"✅ User {id} updated successfully. New RowVersion: {newRowVersion}");
                
                return Ok(new 
                { 
                    message = "User updated successfully",
                    rowVersion = newRowVersion
                });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError($"❌ Concurrency exception for user {id}: {ex.Message}");
                
                return Conflict(new 
                { 
                    message = "⚠️ CONFLICT: User was modified by another admin during save. Please refresh and try again.",
                    conflict = true
                });
            }
        }

        [HttpPut("{id}/toggle-admin")]
        public async Task<ActionResult<UserResponseDto>> ToggleAdmin(int id, [FromBody] ToggleAdminDto dto)
        {
            _logger.LogInformation($"🔄 Toggling admin for user {id} with RowVersion: {dto.RowVersion}");

            var userInDb = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (userInDb == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // ✅ Check for conflicts
            if (userInDb.RowVersion != dto.RowVersion)
            {
                _logger.LogWarning($"⚠️ CONFLICT: RowVersion mismatch for user {id}");
                
                return Conflict(new 
                { 
                    message = "⚠️ CONFLICT: User was modified by another admin. Please refresh the page.",
                    conflict = true
                });
            }

            // ✅ Create updated entity
            var user = new User
            {
                Id = id,
                FirstName = userInDb.FirstName,
                LastName = userInDb.LastName,
                Email = userInDb.Email,
                PhoneNumber = userInDb.PhoneNumber,
                PasswordHash = userInDb.PasswordHash,
                IsAdmin = !userInDb.IsAdmin, // Toggle
                RowVersion = dto.RowVersion
            };

            _context.Users.Attach(user);
            _context.Entry(user).State = EntityState.Modified;
            
            // Update only IsAdmin
            _context.Entry(user).Property(u => u.Email).IsModified = false;
            _context.Entry(user).Property(u => u.PasswordHash).IsModified = false;

            // Generate new RowVersion
            var newRowVersion = Guid.NewGuid().ToString();
            user.RowVersion = newRowVersion;

            try
            {
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"✅ User {id} admin status toggled. New RowVersion: {newRowVersion}");

                return Ok(new UserResponseDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsAdmin = user.IsAdmin,
                    RowVersion = newRowVersion
                });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError($"❌ Concurrency exception: {ex.Message}");
                
                return Conflict(new 
                { 
                    message = "⚠️ CONFLICT: User was modified during save. Please refresh and try again.",
                    conflict = true
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id, [FromBody] DeleteUserDto dto)
        {
            _logger.LogInformation($"🗑️ Deleting user {id} with RowVersion: {dto.RowVersion}");

            var userInDb = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (userInDb == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (userInDb.IsAdmin)
            {
                return BadRequest(new { message = "Cannot delete admin users" });
            }

            // ✅ Check for conflicts
            if (userInDb.RowVersion != dto.RowVersion)
            {
                _logger.LogWarning($"⚠️ CONFLICT: RowVersion mismatch for user {id}");
                
                return Conflict(new 
                { 
                    message = "⚠️ CONFLICT: User was modified by another admin. Please refresh the page.",
                    conflict = true
                });
            }

            // ✅ Create entity for deletion
            var user = new User
            {
                Id = id,
                RowVersion = dto.RowVersion,
                // Other properties don't matter for deletion
                FirstName = userInDb.FirstName,
                LastName = userInDb.LastName,
                Email = userInDb.Email,
                PhoneNumber = userInDb.PhoneNumber,
                PasswordHash = userInDb.PasswordHash,
                IsAdmin = userInDb.IsAdmin
            };

            _context.Users.Attach(user);
            _context.Entry(user).State = EntityState.Deleted;

            try
            {
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"✅ User {id} deleted successfully");
                
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError($"❌ Concurrency exception: {ex.Message}");
                
                return Conflict(new 
                { 
                    message = "⚠️ CONFLICT: User was modified during deletion. Please refresh and try again.",
                    conflict = true
                });
            }
        }
    }

    // DTOs for operations requiring RowVersion
    public class ToggleAdminDto
    {
        public string RowVersion { get; set; }
    }

    public class DeleteUserDto
    {
        public string RowVersion { get; set; }
    }
}