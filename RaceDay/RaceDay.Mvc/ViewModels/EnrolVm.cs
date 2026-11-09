using System.ComponentModel.DataAnnotations;
using RaceDay.Contracts;

namespace RaceDay.Mvc.ViewModels;

public class EnrolVm
{
    public EventDto Event { get; set; } = new();

    [Required]
    [Display(Name = "Category")]
    public int CategoryID { get; set; }
}
