using System;
using CreateCrmWorkItem.Api.Extensions;
using CreateCrmWorkItem.Api.Infrastructure.Rbac;
using CreateCrmWorkItem.Api.Models.Common;
using CreateCrmWorkItem.Api.Models.Responses;
using CreateCrmWorkItem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CreateCrmWorkItem.Api.Controllers;

[ApiController]
[Route("api/v1/crm/wi-categories")]
[Authorize]
public class WiCategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IRbacService _rbacService;
    private readonly PermissionOptions _permissionOptions;

    public WiCategoriesController(ICategoryService categoryService, IRbacService rbacService, IOptions<PermissionOptions> permissionOptions)
    {
        _categoryService = categoryService;
        _rbacService = rbacService;
        _permissionOptions = permissionOptions.Value;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CategoryDto>>> GetAsync([FromQuery(Name = "level")] int level = 1, [FromQuery] string? domain = null, [FromQuery(Name = "q")] string? keyword = null, [FromQuery] int page = 1, [FromQuery(Name = "page_size")] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var userId = User.RequireUserId();
        var orgId = User.RequireOrgId();
        await _rbacService.EnsureAnyPermissionAsync(userId, new[] { _permissionOptions.CrmCategoryRead }, cancellationToken);

        var result = await _categoryService.GetRootCategoriesAsync(orgId, level, domain, keyword, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{categoryId:guid}/children")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetChildrenAsync(Guid categoryId, [FromQuery(Name = "q")] string? keyword = null, CancellationToken cancellationToken = default)
    {
        var userId = User.RequireUserId();
        var orgId = User.RequireOrgId();
        await _rbacService.EnsureAnyPermissionAsync(userId, new[] { _permissionOptions.CrmCategoryRead }, cancellationToken);

        var items = await _categoryService.GetSubcategoriesAsync(orgId, categoryId, keyword, cancellationToken);
        return Ok(new { items });
    }
}
