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
    public class AuthController : ControllerBase
    {
        private readonly CinemaDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public AuthController(CinemaDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(UserRegistrationDto dto)
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
                IsAdmin = false,
                RowVersion = Guid.NewGuid().ToString() 
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

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

        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDto>> Login(UserLoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

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
    }
}