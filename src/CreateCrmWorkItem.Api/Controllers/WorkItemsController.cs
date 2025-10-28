using System;
using CreateCrmWorkItem.Api.Extensions;
using CreateCrmWorkItem.Api.Infrastructure.Rbac;
using CreateCrmWorkItem.Api.Models.Requests;
using CreateCrmWorkItem.Api.Models.Responses;
using CreateCrmWorkItem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CreateCrmWorkItem.Api.Controllers;

[ApiController]
[Route("api/v1/crm/work-items")]
[Authorize]
public class WorkItemsController : ControllerBase
{
    private readonly IWorkItemService _workItemService;
    private readonly IRbacService _rbacService;
    private readonly PermissionOptions _permissionOptions;

    public WorkItemsController(IWorkItemService workItemService, IRbacService rbacService, IOptions<PermissionOptions> permissionOptions)
    {
        _workItemService = workItemService;
        _rbacService = rbacService;
        _permissionOptions = permissionOptions.Value;
    }

    [HttpPost]
    [ProducesResponseType(typeof(WorkItemCreatedResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<WorkItemCreatedResponse>> CreateAsync([FromBody] WorkItemCreateRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = User.RequireUserId();
        var orgId = User.RequireOrgId();
        await _rbacService.EnsureAnyPermissionAsync(userId, new[] { _permissionOptions.CrmWorkItemCreate, _permissionOptions.CrmWorkItemAdmin }, cancellationToken);

        var result = await _workItemService.CreateAsync(userId, orgId, request, idempotencyKey, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkItemDetailResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkItemDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = User.RequireUserId();
        var orgId = User.RequireOrgId();
        await _rbacService.EnsureAnyPermissionAsync(userId, new[] { _permissionOptions.CrmWorkItemRead, _permissionOptions.CrmWorkItemAdmin }, cancellationToken);

        var result = await _workItemService.GetByIdAsync(orgId, id, cancellationToken);
        return Ok(result);
    }
}
