using System;
using CreateCrmWorkItem.Api.Extensions;
using CreateCrmWorkItem.Api.Infrastructure.Rbac;
using CreateCrmWorkItem.Api.Models.Responses;
using CreateCrmWorkItem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CreateCrmWorkItem.Api.Controllers;

[ApiController]
[Route("api/v1/crm/templates")]
[Authorize]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IRbacService _rbacService;
    private readonly PermissionOptions _permissionOptions;

    public TemplatesController(ITemplateService templateService, IRbacService rbacService, IOptions<PermissionOptions> permissionOptions)
    {
        _templateService = templateService;
        _rbacService = rbacService;
        _permissionOptions = permissionOptions.Value;
    }

    [HttpGet("suggest")]
    [ProducesResponseType(typeof(TemplateSuggestResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TemplateSuggestResponse?>> SuggestAsync([FromQuery] string type, [FromQuery] string? domain = null, [FromQuery] Guid? category = null, [FromQuery] Guid? subcategory = null, CancellationToken cancellationToken = default)
    {
        var userId = User.RequireUserId();
        var orgId = User.RequireOrgId();
        await _rbacService.EnsureAnyPermissionAsync(userId, new[] { _permissionOptions.CrmTemplateSuggestRead }, cancellationToken);

        var result = await _templateService.SuggestAsync(orgId, type, category, subcategory, domain, cancellationToken);
        return Ok(result);
    }
}
