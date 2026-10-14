using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict(new { message = "Email already exists." });

            var allowedRoles = new[] { "Organiser", "Participant" };
            var normalizedRole = dto.Role?.Trim();

            if (string.IsNullOrEmpty(normalizedRole) ||
                !allowedRoles.Any(r => string.Equals(r, normalizedRole, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { message = $"Invalid role. Allowed values: {string.Join(", ", allowedRoles)}." });
            }

            // Normalize the role to the canonical casing used by the database/application
            normalizedRole = allowedRoles.First(r => string.Equals(r, normalizedRole, StringComparison.OrdinalIgnoreCase));

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                Role = normalizedRole,
                Phone = dto.Phone
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (dto.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
            {
                _context.Organisers.Add(new Organiser
                {
                    UserId = user.UserId,
                    OrganisationName = dto.OrganisationName ?? $"{dto.FirstName}'s Organisation"
                });
            }
            else if (dto.Role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
            {
                _context.ParticipantProfiles.Add(new ParticipantProfile
                {
                    UserId = user.UserId,
                    DateOfBirth = dto.DateOfBirth ?? DateTime.UtcNow.AddYears(-20)
                });
            }

            await _context.SaveChangesAsync();
            return StatusCode(201, new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials." });

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", user.Role);

            return Ok(new { message = "Login successful.", role = user.Role, userId = user.UserId });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }
    }
}