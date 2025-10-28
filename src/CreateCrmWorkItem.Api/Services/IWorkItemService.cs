using System;
using System.Threading;
using System.Threading.Tasks;
using CreateCrmWorkItem.Api.Models.Requests;
using CreateCrmWorkItem.Api.Models.Responses;

namespace CreateCrmWorkItem.Api.Services;

public interface IWorkItemService
{
    Task<WorkItemCreatedResponse> CreateAsync(Guid userId, Guid orgId, WorkItemCreateRequest request, string? idempotencyKey, CancellationToken cancellationToken);
    Task<WorkItemDetailResponse> GetByIdAsync(Guid orgId, Guid workItemId, CancellationToken cancellationToken);
}
