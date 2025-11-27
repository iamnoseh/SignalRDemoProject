using System.Security.Claims;

namespace WebApp.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }
    public static string GetUserName(this ClaimsPrincipal principal)
    {
        return principal.Identity?.Name ?? "Anonymous";
    }
}
