using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Helpers;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly RaceDayApiClient _api;

    public HomeController(RaceDayApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        // Public landing – show upcoming open events
        var events = await _api.GetEventsAsync("Open") ?? new();
        var upcoming = events
            .Where(e => e.EventDate >= DateTime.Today)
            .OrderBy(e => e.EventDate)
            .Take(6)
            .ToList();
        return View(upcoming);
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}
