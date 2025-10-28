using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CreateCrmWorkItem.Api.Models.Responses;

namespace CreateCrmWorkItem.Api.Services;

public interface IUserSearchService
{
    Task<IReadOnlyList<UserSummaryDto>> SearchAsync(Guid orgId, string? keyword, int limit, CancellationToken cancellationToken);
}
