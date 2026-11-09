using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Mvc.ViewModels;

public class EventFormVm
{
    public int? EventID { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Event Name")]
    public string EventName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Event Date")]
    [DataType(DataType.DateTime)]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(14);

    [Required, MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required, Range(0.1, 500)]
    [Display(Name = "Distance (km)")]
    public decimal DistanceKm { get; set; }

    [Required]
    [Display(Name = "Event Type")]
    public int EventTypeID { get; set; } = 1;

    [Display(Name = "Max Participants")]
    public int? MaxParticipants { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; } = "Open";

    public string? BannerImageUrl { get; set; }

    [Display(Name = "Banner Image")]
    public IFormFile? BannerFile { get; set; }
}
