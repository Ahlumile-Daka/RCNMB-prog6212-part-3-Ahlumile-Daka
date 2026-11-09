using System.ComponentModel.DataAnnotations;
using RaceDay.Contracts;

namespace RaceDay.Mvc.ViewModels;

public class ResultEntryVm
{
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public List<EnrolmentDto> Enrolments { get; set; } = new();
    public List<ResultDto> ExistingResults { get; set; } = new();

    // For new entry form
    public int EnrolmentID { get; set; }

    [Display(Name = "Finish Time (hh:mm:ss)")]
    public string? FinishTime { get; set; }

    [Display(Name = "Position")]
    public int? Position { get; set; }

    public string? Notes { get; set; }
}
