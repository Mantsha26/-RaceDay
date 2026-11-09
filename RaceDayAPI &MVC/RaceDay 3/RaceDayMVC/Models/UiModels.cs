using System.ComponentModel.DataAnnotations;
using System.Globalization;
namespace RaceDayMVC.Models;

public class LoginForm
{
    [Required(ErrorMessage = "Enter your email address."), EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Enter your password.")]
    public string Password { get; set; } = "";
}

public class RegisterForm
{
    [Required(ErrorMessage = "Enter your first name.")] public string FirstName { get; set; } = "";
    [Required(ErrorMessage = "Enter your last name.")] public string LastName { get; set; } = "";
    [Required(ErrorMessage = "Enter your email address."), EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Choose a password."), MinLength(6, ErrorMessage = "Use at least 6 characters.")]
    public string Password { get; set; } = "";
    public string Role { get; set; } = "Participant";
    public string? OrganisationName { get; set; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class RaceEvent
{
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public string? Description { get; set; }
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "Road";
    public string? EventImageUrl { get; set; }
    public int EnrolmentCount { get; set; }
    public List<RaceCategory> Categories { get; set; } = new();
}

public class RaceCategory
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int MinAge { get; set; }
    public int MaxAge { get; set; } = 99;
}

public class RaceEnrolment
{
    public int EnrolmentId { get; set; }
    public string Status { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string ParticipantName { get; set; } = "";
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public DateTime? EnrolmentDate { get; set; }
    public int EventId { get; set; }
    public int? ResultId { get; set; }
    public int? Position { get; set; }
    public decimal? TimeSeconds { get; set; }
    public int Finishers { get; set; }
}

public class ProfileForm
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Gender { get; set; }
    public string? EmergencyContact { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class CategoriesPage
{
    public RaceEvent Event { get; set; } = new();
    public List<RaceCategory> Categories { get; set; } = new();
    public RaceCategory Form { get; set; } = new();
}

public class EnrolmentsPage
{
    public RaceEvent Event { get; set; } = new();
    public List<RaceEnrolment> Enrolments { get; set; } = new();
}

public class ResultsPage
{
    public RaceEvent Event { get; set; } = new();
    public List<RaceEnrolment> Enrolments { get; set; } = new();
    /// <summary>Results can only be captured once the event date has arrived.</summary>
    public bool Open { get; set; }
}

public static class RaceFormat
{
    public static string Time(decimal? seconds)
    {
        if (seconds is null) return "—";
        var t = TimeSpan.FromSeconds((double)seconds.Value);
        return $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
    }

    /// <summary>Time in the form the organiser types it: h:mm:ss, with optional decimals on the seconds.</summary>
    public static string TimeInput(decimal? seconds)
    {
        if (seconds is null) return "";
        var t = TimeSpan.FromSeconds((double)seconds.Value);
        var secs = t.Seconds + (seconds.Value - Math.Floor(seconds.Value));
        return $"{(int)t.TotalHours}:{t.Minutes:00}:{secs.ToString("00.##", CultureInfo.InvariantCulture)}";
    }

    public static decimal? ParseTime(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var parts = text.Trim().Split(':');
        if (parts.Length < 2 || parts.Length > 3) return null;
        if (!decimal.TryParse(parts[^1], NumberStyles.Number, CultureInfo.InvariantCulture, out var secs) || secs < 0 || secs >= 60) return null;
        if (!int.TryParse(parts[^2], out var mins) || mins < 0 || (parts.Length == 3 && mins >= 60)) return null;
        var hours = 0;
        if (parts.Length == 3 && (!int.TryParse(parts[0], out hours) || hours < 0)) return null;
        var total = hours * 3600m + mins * 60m + secs;
        return total > 0 ? total : null;
    }

    public static string Ordinal(int n)
    {
        var rem100 = n % 100;
        if (rem100 is >= 11 and <= 13) return $"{n}th";
        return (n % 10) switch { 1 => $"{n}st", 2 => $"{n}nd", 3 => $"{n}rd", _ => $"{n}th" };
    }
}
