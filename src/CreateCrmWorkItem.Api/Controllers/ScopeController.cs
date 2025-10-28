using System;
using CreateCrmWorkItem.Api.Extensions;
using CreateCrmWorkItem.Api.Infrastructure.Rbac;
using CreateCrmWorkItem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CreateCrmWorkItem.Api.Controllers;

[ApiController]
[Route("api/v1/crm/scope")]
[Authorize]
public class ScopeController : ControllerBase
{
    private readonly IScopeSearchService _scopeSearchService;
    private readonly IRbacService _rbacService;
    private readonly PermissionOptions _permissionOptions;

    public ScopeController(IScopeSearchService scopeSearchService, IRbacService rbacService, IOptions<PermissionOptions> permissionOptions)
    {
        _scopeSearchService = scopeSearchService;
        _rbacService = rbacService;
        _permissionOptions = permissionOptions.Value;
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> SearchAsync([FromQuery] string type, [FromQuery(Name = "q")] string? keyword = null, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var userId = User.RequireUserId();
        var orgId = User.RequireOrgId();
        var permissions = type switch
        {
            "project" => new[] { _permissionOptions.CrmScopeProjectRead },
            "department" => new[] { _permissionOptions.CrmScopeDepartmentRead },
            "user" => new[] { _permissionOptions.CrmScopeUserRead },
            _ => Array.Empty<string>()
        };

        await _rbacService.EnsureAnyPermissionAsync(userId, permissions, cancellationToken);

        var items = await _scopeSearchService.SearchAsync(orgId, type, keyword, limit, cancellationToken);
        return Ok(new { items });
    }
}
