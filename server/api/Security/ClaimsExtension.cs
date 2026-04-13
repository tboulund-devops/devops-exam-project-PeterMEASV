using System.Security.Claims;
using efscaffold;

namespace api.Security;

public static class ClaimExtensions
{
    public static string GetUserId(this ClaimsPrincipal claims)
    {
        var id =
            claims.FindFirst("sub")?.Value
            ?? claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(id))
            throw new UnauthorizedAccessException("Missing user id claim (sub/nameidentifier).");

        return id;
    }

    public static IEnumerable<Claim> ToClaims(this User user) =>
        [new("sub", user.Id.ToString())];

    public static ClaimsPrincipal ToPrincipal(this User user) =>
        new ClaimsPrincipal(new ClaimsIdentity(user.ToClaims()));
}