using BCrypt.Net;
using Microsoft.AspNetCore.DataProtection;
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
            var email = dto.Email.Trim();
            var allowedRoles = new[] { "Organiser", "Participant" };
            var normalizedRole = allowedRoles.FirstOrDefault(r =>
                string.Equals(r, dto.Role?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (normalizedRole is null)
                return BadRequest(new { message = "Role must be Organiser or Participant." });

            if (normalizedRole == "Participant" &&
                (dto.DateOfBirth is null || dto.DateOfBirth.Value.Date >= DateTime.UtcNow.Date))
                return BadRequest(new { message = "Participants must provide a valid date of birth." });

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return Conflict(new { message = "Email already exists." });

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    FirstName = dto.FirstName.Trim(),
                    LastName = dto.LastName.Trim(),
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = normalizedRole,
                    Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim()
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (normalizedRole == "Organiser")
                {
                    _context.Organisers.Add(new Organiser
                    {
                        UserId = user.UserId,
                        OrganisationName = string.IsNullOrWhiteSpace(dto.OrganisationName)
                            ? $"{user.FirstName}'s Organisation"
                            : dto.OrganisationName.Trim()
                    });
                }
                else
                {
                    _context.ParticipantProfiles.Add(new ParticipantProfile
                    {
                        UserId = user.UserId,
                        DateOfBirth = dto.DateOfBirth!.Value
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(StatusCodes.Status201Created,
                    new { message = "User registered successfully.", userId = user.UserId, role = user.Role });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.Trim());
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials." });

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", user.Role);

            var protector=HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>().CreateProtector("RaceDay.ApiSession.v1");
            var token=protector.Protect($"{user.UserId}|{user.Role}|{DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds()}");
            return Ok(new { message = "Login successful.", role = user.Role, userId = user.UserId, token });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }
    }
}