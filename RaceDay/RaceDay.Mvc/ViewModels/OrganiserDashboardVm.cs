using RaceDay.Contracts;

namespace RaceDay.Mvc.ViewModels;

public class OrganiserDashboardVm
{
    public string FullName { get; set; } = string.Empty;
    public List<EventDto> Events { get; set; } = new();
    public int TotalEvents => Events.Count;
    public int TotalEnrolments { get; set; }
    public List<EventDto> UpcomingEvents => Events
        .Where(e => e.EventDate >= DateTime.Today)
        .OrderBy(e => e.EventDate)
        .Take(5)
        .ToList();
}
