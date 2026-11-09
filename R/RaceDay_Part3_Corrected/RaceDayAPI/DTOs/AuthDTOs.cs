using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs
{
    public class RegisterDto
    {
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
        [Required] public string Role { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? OrganisationName { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    public class LoginDto
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }
}
