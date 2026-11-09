using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static UserProfileDto MapProfile(User user) => new()
        {
            UserID = user.UserID,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            Phone = user.Phone,
            DateRegistered = user.DateRegistered,
            IsActive = user.IsActive,
            ProfileImageUrl = user.ProfileImageUrl
        };

        /// <summary>
        /// Returns the profile of the currently logged-in user.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            return Ok(MapProfile(user));
        }

        /// <summary>
        /// Updates the logged-in user's profile (name, phone, etc.).
        /// </summary>
        [HttpPut("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName;
            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName;
            if (request.Phone != null)
                user.Phone = request.Phone;

            await _context.SaveChangesAsync();

            return Ok(MapProfile(user));
        }

        /// <summary>
        /// Uploads a profile picture. Stored in Azure Blob Storage.
        /// </summary>
        [HttpPost("me/profile-image")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadProfileImage(IFormFile file, [FromServices] IBlobService blobService)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowed.Contains(file.ContentType.ToLowerInvariant()))
                return BadRequest(new { message = "Only JPEG, PNG, GIF or WebP images are allowed." });

            if (file.Length > 3 * 1024 * 1024)
                return BadRequest(new { message = "File size must be under 3 MB." });

            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                await blobService.DeleteAsync(user.ProfileImageUrl);

            await using var stream = file.OpenReadStream();
            var url = await blobService.UploadAsync(stream, file.FileName, file.ContentType);

            user.ProfileImageUrl = url;
            await _context.SaveChangesAsync();

            return Ok(MapProfile(user));
        }

        /// <summary>
        /// Returns a public summary of a user (useful for organiser view).
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PublicUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || !user.IsActive)
                return NotFound(new { message = "User not found." });

            return Ok(new PublicUserDto
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            });
        }
    }
}
