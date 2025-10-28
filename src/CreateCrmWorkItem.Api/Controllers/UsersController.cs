using CreateCrmWorkItem.Api.Extensions;
using CreateCrmWorkItem.Api.Infrastructure.Rbac;
using CreateCrmWorkItem.Api.Models.Responses;
using CreateCrmWorkItem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CreateCrmWorkItem.Api.Controllers;

[ApiController]
[Route("api/v1/crm/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserSearchService _userSearchService;
    private readonly IRbacService _rbacService;
    private readonly PermissionOptions _permissionOptions;

    public UsersController(IUserSearchService userSearchService, IRbacService rbacService, IOptions<PermissionOptions> permissionOptions)
    {
        _userSearchService = userSearchService;
        _rbacService = rbacService;
        _permissionOptions = permissionOptions.Value;
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> SearchAsync([FromQuery(Name = "q")] string? keyword = null, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var userId = User.RequireUserId();
        var orgId = User.RequireOrgId();
        await _rbacService.EnsureAnyPermissionAsync(userId, new[] { _permissionOptions.CrmUserRead, _permissionOptions.CrmDirectoryRead }, cancellationToken);

        var items = await _userSearchService.SearchAsync(orgId, keyword, limit, cancellationToken);
        return Ok(new { items });
    }
}
