using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CreateCrmWorkItem.Api.Exceptions;
using CreateCrmWorkItem.Api.Infrastructure.Database;
using CreateCrmWorkItem.Api.Models.Requests;
using CreateCrmWorkItem.Api.Models.Responses;
using Dapper;

namespace CreateCrmWorkItem.Api.Services;

public class WorkItemService : IWorkItemService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public WorkItemService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<WorkItemCreatedResponse> CreateAsync(Guid userId, Guid orgId, WorkItemCreateRequest request, string? idempotencyKey, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            const string getExistingSql = @"
                select work_item_id as Id,
                       ref_code as RefCode,
                       status,
                       created_at as CreatedAt
                from crm_work_item_idempotency
                where user_id = @UserId and idempotency_key = @Key";

            var existing = await connection.QuerySingleOrDefaultAsync<WorkItemCreatedResponse>(new CommandDefinition(
                getExistingSql,
                new { UserId = userId, Key = idempotencyKey },
                transaction,
                cancellationToken: cancellationToken));

            if (existing is not null)
            {
                return existing;
            }
        }

        var workItemId = Guid.NewGuid();
        var watchers = request.WatcherIds?.Distinct().ToArray() ?? Array.Empty<Guid>();
        var scopeType = request.Scope?.Type;
        var scopeEntityId = request.Scope?.EntityId;

        const string insertSql = @"
            insert into crm_work_items
                (id, org_id, type, domain, category_id, subcategory_id, title, description, priority, due_at, scope_type, scope_entity_id, assignee_id, created_by, created_at, status, ref_code)
            values
                (@Id, @OrgId, @Type, @Domain, @CategoryId, @SubcategoryId, @Title, @Description, @Priority, @DueAt, @ScopeType, @ScopeEntityId, @AssigneeId, @CreatedBy, now(), 'new', crm_generate_ref_code(@OrgId))
            returning ref_code as RefCode, status as Status, created_at as CreatedAt";

        var insertResult = await connection.QuerySingleAsync<CreationRow>(new CommandDefinition(
            insertSql,
            new
            {
                Id = workItemId,
                OrgId = orgId,
                Type = request.Type,
                Domain = request.Domain,
                CategoryId = request.CategoryId,
                SubcategoryId = request.SubcategoryId,
                Title = request.Title.Trim(),
                Description = request.Description,
                Priority = request.Priority ?? "medium",
                DueAt = request.DueAt,
                ScopeType = scopeType,
                ScopeEntityId = scopeEntityId,
                AssigneeId = request.AssigneeId,
                CreatedBy = userId
            },
            transaction,
            cancellationToken: cancellationToken));

        if (watchers.Length > 0)
        {
            const string watcherSql = @"
                insert into crm_work_item_watchers (work_item_id, user_id)
                values (@WorkItemId, @UserId)
                on conflict do nothing";

            foreach (var watcherId in watchers)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    watcherSql,
                    new { WorkItemId = workItemId, UserId = watcherId },
                    transaction,
                    cancellationToken: cancellationToken));
            }
        }

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            const string idempotencySql = @"
                insert into crm_work_item_idempotency (user_id, idempotency_key, work_item_id, ref_code, status, created_at)
                values (@UserId, @Key, @WorkItemId, @RefCode, @Status, @CreatedAt)
                on conflict (user_id, idempotency_key) do nothing";

            await connection.ExecuteAsync(new CommandDefinition(
                idempotencySql,
                new
                {
                    UserId = userId,
                    Key = idempotencyKey,
                    WorkItemId = workItemId,
                    RefCode = insertResult.RefCode,
                    Status = insertResult.Status,
                    CreatedAt = insertResult.CreatedAt
                },
                transaction,
                cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);

        return new WorkItemCreatedResponse(workItemId, insertResult.RefCode, insertResult.Status, insertResult.CreatedAt);
    }

    public async Task<WorkItemDetailResponse> GetByIdAsync(Guid orgId, Guid workItemId, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        const string detailSql = @"
            select wi.id,
                   wi.ref_code,
                   wi.type as RefType,
                   wi.title,
                   wi.status,
                   wi.priority,
                   wi.due_at,
                   wi.created_at,
                   c.id as CategoryId,
                   c.name as CategoryName,
                   s.id as SubcategoryId,
                   s.name as SubcategoryName,
                   a.id as AssigneeId,
                   a.full_name as AssigneeName
            from crm_work_items wi
            left join crm_categories c on c.id = wi.category_id
            left join crm_subcategories s on s.id = wi.subcategory_id
            left join crm_users a on a.id = wi.assignee_id
            where wi.org_id = @OrgId and wi.id = @Id";

        var workItem = await connection.QuerySingleOrDefaultAsync<WorkItemRow>(new CommandDefinition(
            detailSql,
            new { OrgId = orgId, Id = workItemId },
            cancellationToken: cancellationToken));

        if (workItem is null)
        {
            throw new NotFoundException("work item not found");
        }

        const string watcherSql = @"
            select u.id, u.full_name as FullName
            from crm_work_item_watchers w
            join crm_users u on u.id = w.user_id
            where w.work_item_id = @Id
            order by u.full_name asc";

        var watcherList = await connection.QueryAsync<WorkItemDetailResponse.UserSummary>(new CommandDefinition(
            watcherSql,
            new { Id = workItemId },
            cancellationToken: cancellationToken));

        return new WorkItemDetailResponse
        {
            Id = workItem.Id,
            RefCode = workItem.RefCode,
            RefType = workItem.RefType,
            Title = workItem.Title,
            Status = workItem.Status,
            Priority = workItem.Priority,
            DueAt = workItem.DueAt,
            CreatedAt = workItem.CreatedAt,
            Category = workItem.CategoryId is null ? null : new WorkItemDetailResponse.CategorySummary(workItem.CategoryId.Value, workItem.CategoryName!),
            Subcategory = workItem.SubcategoryId is null ? null : new WorkItemDetailResponse.SubcategorySummary(workItem.SubcategoryId.Value, workItem.SubcategoryName!),
            Assignee = workItem.AssigneeId is null ? null : new WorkItemDetailResponse.UserSummary(workItem.AssigneeId.Value, workItem.AssigneeName!),
            Watchers = watcherList.ToList()
        };
    }

    private static void ValidateRequest(WorkItemCreateRequest request)
    {
        if (request.Scope is not null && string.IsNullOrWhiteSpace(request.Scope.Type))
        {
            throw new BadRequestException("scope.type is required when scope is provided");
        }

        if (request.Scope is not null && request.Scope.EntityId == Guid.Empty)
        {
            throw new BadRequestException("scope.entity_id is required when scope is provided");
        }

        if (request.WatcherIds != null && request.WatcherIds.Count > 100)
        {
            throw new BadRequestException("watcher_ids maximum is 100");
        }
    }

    private sealed record CreationRow(string RefCode, string Status, DateTimeOffset CreatedAt);

    private sealed record WorkItemRow(
        Guid Id,
        string RefCode,
        string RefType,
        string Title,
        string Status,
        string Priority,
        DateTimeOffset? DueAt,
        DateTimeOffset CreatedAt,
        Guid? CategoryId,
        string? CategoryName,
        Guid? SubcategoryId,
        string? SubcategoryName,
        Guid? AssigneeId,
        string? AssigneeName);
}
