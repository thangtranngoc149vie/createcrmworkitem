using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CreateCrmWorkItem.Api.Models.Common;
using CreateCrmWorkItem.Api.Models.Responses;

namespace CreateCrmWorkItem.Api.Services;

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> GetRootCategoriesAsync(Guid orgId, int level, string? domain, string? keyword, int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<SubcategoryDto>> GetSubcategoriesAsync(Guid orgId, Guid categoryId, string? keyword, CancellationToken cancellationToken);
}
