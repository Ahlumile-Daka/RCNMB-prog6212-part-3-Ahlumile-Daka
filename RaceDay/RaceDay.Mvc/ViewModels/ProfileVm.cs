using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Mvc.ViewModels;

public class ProfileVm
{
    public int UserID { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    public DateTime DateRegistered { get; set; }

    public string? ProfileImageUrl { get; set; }

    [Display(Name = "Profile Picture")]
    public IFormFile? ProfileImageFile { get; set; }
}
