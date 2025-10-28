using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CreateCrmWorkItem.Api.Models.Responses;

namespace CreateCrmWorkItem.Api.Services;

public interface IScopeSearchService
{
    Task<IReadOnlyList<ScopeItemDto>> SearchAsync(Guid orgId, string type, string? keyword, int limit, CancellationToken cancellationToken);
}
