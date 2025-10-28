using System;
using System.Threading;
using System.Threading.Tasks;
using CreateCrmWorkItem.Api.Infrastructure.Database;
using CreateCrmWorkItem.Api.Models.Responses;
using Dapper;

namespace CreateCrmWorkItem.Api.Services;

public class TemplateService : ITemplateService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TemplateService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TemplateSuggestResponse?> SuggestAsync(Guid orgId, string type, Guid? categoryId, Guid? subcategoryId, string? domain, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        const string sql = @"
            select template_code as TemplateCode,
                   assignment_mode as Mode,
                   assignment_user_id as AssigneeId,
                   assignment_display as Display,
                   scope_type as ScopeType,
                   scope_entity_id as ScopeEntityId,
                   scope_display as ScopeDisplay,
                   due_in_days as DueInDays
            from crm_template_suggestions
            where org_id = @OrgId
              and type = @Type
              and (@Domain is null or domain = @Domain)
              and (@CategoryId is null or category_id = @CategoryId)
              and (@SubcategoryId is null or subcategory_id = @SubcategoryId)
            order by priority desc, updated_at desc
            limit 1";

        var result = await connection.QuerySingleOrDefaultAsync<TemplateRow>(new CommandDefinition(sql, new
        {
            OrgId = orgId,
            Type = type,
            Domain = domain,
            CategoryId = categoryId,
            SubcategoryId = subcategoryId
        }, cancellationToken: cancellationToken));

        if (result == null)
        {
            return null;
        }

        return new TemplateSuggestResponse(
            result.TemplateCode,
            result.Mode is null && result.AssigneeId is null && result.Display is null
                ? null
                : new TemplateSuggestAssignment(result.Mode!, result.AssigneeId, result.Display),
            result.ScopeType is null && result.ScopeEntityId is null && result.ScopeDisplay is null
                ? null
                : new TemplateSuggestScope(result.ScopeType, result.ScopeEntityId, result.ScopeDisplay),
            result.DueInDays);
    }

    private sealed record TemplateRow(
        string TemplateCode,
        string? Mode,
        Guid? AssigneeId,
        string? Display,
        string? ScopeType,
        Guid? ScopeEntityId,
        string? ScopeDisplay,
        int? DueInDays);
}
