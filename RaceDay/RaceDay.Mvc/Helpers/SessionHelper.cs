namespace RaceDay.Mvc.Helpers;

public static class SessionHelper
{
    public static bool IsAuthenticated(HttpContext ctx) =>
        !string.IsNullOrEmpty(ctx.Session.GetString("JwtToken"));

    public static int? GetUserId(HttpContext ctx) =>
        ctx.Session.GetInt32("UserId");

    public static string? GetRole(HttpContext ctx) =>
        ctx.Session.GetString("Role");

    public static string? GetFullName(HttpContext ctx) =>
        ctx.Session.GetString("FullName");

    public static bool IsOrganiser(HttpContext ctx) =>
        GetRole(ctx) == "Organiser";

    public static bool IsParticipant(HttpContext ctx) =>
        GetRole(ctx) == "Participant";
}
