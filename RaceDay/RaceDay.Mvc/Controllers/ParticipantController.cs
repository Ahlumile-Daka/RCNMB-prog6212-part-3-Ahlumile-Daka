using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Helpers;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

[AuthorizeSession("Participant")]
public class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api;

    public ParticipantController(RaceDayApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var events = await _api.GetEventsAsync("Open") ?? new();
        var upcoming = events
            .Where(e => e.EventDate >= DateTime.Today)
            .OrderBy(e => e.EventDate)
            .ToList();
        return View(upcoming);
    }

    public async Task<IActionResult> EventDetails(int id)
    {
        var evt = await _api.GetEventAsync(id);
        if (evt == null) return NotFound();
        return View(evt);
    }

    [HttpGet]
    public async Task<IActionResult> Enrol(int id)
    {
        var evt = await _api.GetEventAsync(id);
        if (evt == null) return NotFound();
        if (evt.Categories == null || !evt.Categories.Any())
        {
            TempData["Error"] = "This event has no categories yet. Please try again later.";
            return RedirectToAction(nameof(EventDetails), new { id });
        }

        var vm = new EnrolVm { Event = evt };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int id, EnrolVm model)
    {
        if (model.CategoryID <= 0)
        {
            ModelState.AddModelError("CategoryID", "Please select a category.");
            model.Event = await _api.GetEventAsync(id) ?? new();
            return View(model);
        }

        var request = new CreateEnrolmentRequest
        {
            EventID = id,
            CategoryID = model.CategoryID
        };

        var (success, _, error) = await _api.CreateEnrolmentAsync(request);
        if (!success)
        {
            TempData["Error"] = error ?? "Enrolment failed.";
            return RedirectToAction(nameof(EventDetails), new { id });
        }

        TempData["Success"] = "You have successfully enrolled!";
        return RedirectToAction(nameof(MyEvents));
    }

    public async Task<IActionResult> MyEvents()
    {
        var enrolments = await _api.GetMyEnrolmentsAsync() ?? new();
        return View(enrolments);
    }

    public async Task<IActionResult> MyResults()
    {
        var results = await _api.GetMyResultsAsync() ?? new();
        return View(results);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var profile = await _api.GetMyProfileAsync();
        if (profile == null) return RedirectToAction("Login", "Auth");

        var vm = new ProfileVm
        {
            UserID = profile.UserID,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Email = profile.Email,
            Role = profile.Role,
            Phone = profile.Phone,
            DateRegistered = profile.DateRegistered,
            ProfileImageUrl = profile.ProfileImageUrl
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileVm model)
    {
        if (!ModelState.IsValid) return View(model);

        var request = new UpdateProfileRequest
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Phone = model.Phone
        };

        var (success, data, error) = await _api.UpdateMyProfileAsync(request);
        if (!success)
        {
            ModelState.AddModelError("", error ?? "Update failed.");
            return View(model);
        }

        if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
        {
            var (imgOk, imgData, imgErr) = await _api.UploadProfileImageAsync(model.ProfileImageFile);
            if (imgOk && imgData != null)
                model.ProfileImageUrl = imgData.ProfileImageUrl;
            else if (!imgOk)
                TempData["Error"] = imgErr ?? "Profile image upload failed.";
        }

        // Refresh session name
        if (data != null)
            HttpContext.Session.SetString("FullName", $"{data.FirstName} {data.LastName}");

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }
}
