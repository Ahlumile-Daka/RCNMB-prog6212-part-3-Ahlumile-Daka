namespace RaceDay.Contracts;

// ========== AUTH ==========
public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Organiser | Participant
    public string? Phone { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

// ========== PROFILE / USERS ==========
public class UserProfileDto
{
    public int UserID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime DateRegistered { get; set; }
    public bool IsActive { get; set; }
    public string? ProfileImageUrl { get; set; }
}

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
}

public class PublicUserDto
{
    public int UserID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

// ========== EVENTS ==========
public class CreateEventRequest
{
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int EventTypeID { get; set; }
    public int? MaxParticipants { get; set; }
    public string? Status { get; set; }
}

public class UpdateEventRequest
{
    public string? EventName { get; set; }
    public string? Description { get; set; }
    public DateTime? EventDate { get; set; }
    public string? Location { get; set; }
    public decimal? DistanceKm { get; set; }
    public int? EventTypeID { get; set; }
    public int? MaxParticipants { get; set; }
    public string? Status { get; set; }
}

public class EventDto
{
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int EventTypeID { get; set; }
    public string? EventTypeName { get; set; }
    public int OrganiserID { get; set; }
    public string? OrganiserName { get; set; }
    public int? MaxParticipants { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? BannerImageUrl { get; set; }
    public List<CategoryDto>? Categories { get; set; }
}

// ========== CATEGORIES ==========
public class CreateCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal EntryFee { get; set; }
    public int? MaxEntries { get; set; }
}

public class UpdateCategoryRequest
{
    public string? CategoryName { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? EntryFee { get; set; }
    public int? MaxEntries { get; set; }
}

public class CategoryDto
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal EntryFee { get; set; }
    public int? MaxEntries { get; set; }
}

// ========== ENROLMENTS ==========
public class CreateEnrolmentRequest
{
    public int EventID { get; set; }
    public int CategoryID { get; set; }
}

public class EnrolmentDto
{
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public string? ParticipantName { get; set; }
    public int EventID { get; set; }
    public string? EventName { get; set; }
    public int CategoryID { get; set; }
    public string? CategoryName { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpdateEnrolmentStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

// ========== RESULTS ==========
public class CreateResultRequest
{
    public int EnrolmentID { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public string? Notes { get; set; }
}

public class UpdateResultRequest
{
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public string? Notes { get; set; }
}

public class ResultDto
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public int EventID { get; set; }
    public string? EventName { get; set; }
    public string? CategoryName { get; set; }
    public int ParticipantID { get; set; }
    public string? ParticipantName { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public string? Notes { get; set; }
    public int CapturedBy { get; set; }
    public DateTime CapturedDate { get; set; }
}
