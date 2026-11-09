using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Helpers;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

[AuthorizeSession("Organiser")]
public class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api;

    public OrganiserController(RaceDayApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Dashboard()
    {
        var allEvents = await _api.GetEventsAsync() ?? new();
        var userId = SessionHelper.GetUserId(HttpContext);
        var myEvents = allEvents.Where(e => e.OrganiserID == userId).ToList();

        int totalEnrolments = 0;
        foreach (var evt in myEvents)
        {
            var enrols = await _api.GetEventEnrolmentsAsync(evt.EventID);
            totalEnrolments += enrols?.Count ?? 0;
        }

        var vm = new OrganiserDashboardVm
        {
            FullName = SessionHelper.GetFullName(HttpContext) ?? "Organiser",
            Events = myEvents,
            TotalEnrolments = totalEnrolments
        };
        return View(vm);
    }

    // ========== EVENTS ==========
    public async Task<IActionResult> Events()
    {
        var all = await _api.GetEventsAsync() ?? new();
        var userId = SessionHelper.GetUserId(HttpContext);
        var myEvents = all.Where(e => e.OrganiserID == userId).OrderByDescending(e => e.EventDate).ToList();
        return View(myEvents);
    }

    [HttpGet]
    public IActionResult CreateEvent()
    {
        return View(new EventFormVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventFormVm model)
    {
        if (!ModelState.IsValid) return View(model);

        var request = new CreateEventRequest
        {
            EventName = model.EventName,
            Description = model.Description,
            EventDate = model.EventDate,
            Location = model.Location,
            DistanceKm = model.DistanceKm,
            EventTypeID = model.EventTypeID,
            MaxParticipants = model.MaxParticipants,
            Status = model.Status
        };

        var (success, data, error) = await _api.CreateEventAsync(request);
        if (!success || data == null)
        {
            ModelState.AddModelError("", error ?? "Failed to create event.");
            return View(model);
        }

        // Optional banner upload
        if (model.BannerFile != null && model.BannerFile.Length > 0)
        {
            var (bannerOk, _, bannerError) = await _api.UploadEventBannerAsync(data.EventID, model.BannerFile);
            if (!bannerOk)
                TempData["Error"] = "Event saved, but the banner upload failed: " + (bannerError ?? "unknown error");
        }

        TempData["Success"] = "Event created successfully.";
        return RedirectToAction(nameof(Events));
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var evt = await _api.GetEventAsync(id);
        if (evt == null) return NotFound();
        if (evt.OrganiserID != SessionHelper.GetUserId(HttpContext)) return Forbid();

        var vm = new EventFormVm
        {
            EventID = evt.EventID,
            EventName = evt.EventName,
            Description = evt.Description,
            EventDate = evt.EventDate,
            Location = evt.Location,
            DistanceKm = evt.DistanceKm,
            EventTypeID = evt.EventTypeID,
            MaxParticipants = evt.MaxParticipants,
            Status = evt.Status,
            BannerImageUrl = evt.BannerImageUrl
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(int id, EventFormVm model)
    {
        if (!ModelState.IsValid) return View(model);

        var request = new UpdateEventRequest
        {
            EventName = model.EventName,
            Description = model.Description,
            EventDate = model.EventDate,
            Location = model.Location,
            DistanceKm = model.DistanceKm,
            EventTypeID = model.EventTypeID,
            MaxParticipants = model.MaxParticipants,
            Status = model.Status
        };

        var (success, _, error) = await _api.UpdateEventAsync(id, request);
        if (!success)
        {
            ModelState.AddModelError("", error ?? "Failed to update event.");
            return View(model);
        }

        if (model.BannerFile != null && model.BannerFile.Length > 0)
        {
            var (bannerOk, _, bannerError) = await _api.UploadEventBannerAsync(id, model.BannerFile);
            if (!bannerOk)
                TempData["Error"] = "Event saved, but the banner upload failed: " + (bannerError ?? "unknown error");
        }

        TempData["Success"] = "Event updated successfully.";
        return RedirectToAction(nameof(Events));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var (success, error) = await _api.DeleteEventAsync(id);
        if (!success)
            TempData["Error"] = error ?? "Could not delete event.";
        else
            TempData["Success"] = "Event deleted.";
        return RedirectToAction(nameof(Events));
    }

    // ========== CATEGORIES ==========
    public async Task<IActionResult> Categories(int eventId)
    {
        var evt = await _api.GetEventAsync(eventId);
        if (evt == null) return NotFound();
        if (evt.OrganiserID != SessionHelper.GetUserId(HttpContext)) return Forbid();

        ViewBag.Event = evt;
        var categories = await _api.GetEventCategoriesAsync(eventId) ?? new();
        return View(categories);
    }

    [HttpGet]
    public IActionResult CreateCategory(int eventId)
    {
        return View(new CategoryFormVm { EventID = eventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CategoryFormVm model)
    {
        if (!ModelState.IsValid) return View(model);

        var request = new CreateCategoryRequest
        {
            CategoryName = model.CategoryName,
            MinAge = model.MinAge,
            MaxAge = model.MaxAge,
            EntryFee = model.EntryFee,
            MaxEntries = model.MaxEntries
        };

        var (success, _, error) = await _api.CreateCategoryAsync(model.EventID, request);
        if (!success)
        {
            ModelState.AddModelError("", error ?? "Failed to create category.");
            return View(model);
        }

        TempData["Success"] = "Category added.";
        return RedirectToAction(nameof(Categories), new { eventId = model.EventID });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, int eventId)
    {
        var (success, error) = await _api.DeleteCategoryAsync(id);
        TempData[success ? "Success" : "Error"] = success ? "Category deleted." : (error ?? "Delete failed.");
        return RedirectToAction(nameof(Categories), new { eventId });
    }

    // ========== ENROLMENTS ==========
    public async Task<IActionResult> Enrolments(int eventId)
    {
        var evt = await _api.GetEventAsync(eventId);
        if (evt == null) return NotFound();
        if (evt.OrganiserID != SessionHelper.GetUserId(HttpContext)) return Forbid();

        ViewBag.Event = evt;
        var enrolments = await _api.GetEventEnrolmentsAsync(eventId) ?? new();
        return View(enrolments);
    }

    // ========== RESULTS ==========
    public async Task<IActionResult> Results(int eventId)
    {
        var evt = await _api.GetEventAsync(eventId);
        if (evt == null) return NotFound();
        if (evt.OrganiserID != SessionHelper.GetUserId(HttpContext)) return Forbid();

        var enrolments = await _api.GetEventEnrolmentsAsync(eventId) ?? new();
        var results = await _api.GetEventResultsAsync(eventId) ?? new();

        var vm = new ResultEntryVm
        {
            EventID = eventId,
            EventName = evt.EventName,
            Enrolments = enrolments.Where(e => e.Status == "Confirmed").ToList(),
            ExistingResults = results
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CaptureResult(ResultEntryVm model)
    {
        if (model.EnrolmentID <= 0)
        {
            TempData["Error"] = "Select a participant.";
            return RedirectToAction(nameof(Results), new { eventId = model.EventID });
        }

        TimeSpan? finish = null;
        if (!string.IsNullOrWhiteSpace(model.FinishTime) && TimeSpan.TryParse(model.FinishTime, out var ts))
            finish = ts;

        var request = new CreateResultRequest
        {
            EnrolmentID = model.EnrolmentID,
            FinishTime = finish,
            Position = model.Position,
            Notes = model.Notes
        };

        var (success, _, error) = await _api.CreateResultAsync(request);
        TempData[success ? "Success" : "Error"] = success ? "Result captured." : (error ?? "Failed to capture result.");
        return RedirectToAction(nameof(Results), new { eventId = model.EventID });
    }
}
