using System.Security.Claims;

namespace CreateCrmWorkItem.Api.Extensions;

public static class HttpContextExtensions
{
    public static Guid RequireUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(value, out var id))
        {
            return id;
        }

        throw new Exceptions.UnauthorizedException("missing user id");
    }

    public static Guid RequireOrgId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("OrgId")?.Value;
        if (Guid.TryParse(value, out var id))
        {
            return id;
        }

        throw new Exceptions.UnauthorizedException("missing org id");
    }

    public static Guid? TryRoleId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("RoleId")?.Value;
        if (Guid.TryParse(value, out var id))
        {
            return id;
        }

        return null;
    }
}
