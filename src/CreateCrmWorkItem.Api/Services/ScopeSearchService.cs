using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CreateCrmWorkItem.Api.Exceptions;
using CreateCrmWorkItem.Api.Infrastructure.Database;
using CreateCrmWorkItem.Api.Models.Responses;
using Dapper;

namespace CreateCrmWorkItem.Api.Services;

public class ScopeSearchService : IScopeSearchService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ScopeSearchService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ScopeItemDto>> SearchAsync(Guid orgId, string type, string? keyword, int limit, CancellationToken cancellationToken)
    {
        if (limit <= 0) limit = 10;
        if (limit > 50) limit = 50;

        var sql = type switch
        {
            "project" => @"
                select id, code, name
                from crm_projects
                where org_id = @OrgId
                  and is_archived = false
                  and (@Keyword is null or unaccent(name) ilike unaccent(@Keyword) or code ilike @KeywordRaw)
                order by name asc
                limit @Limit",
            "department" => @"
                select id, code, name
                from crm_departments
                where org_id = @OrgId
                  and is_active = true
                  and (@Keyword is null or unaccent(name) ilike unaccent(@Keyword) or code ilike @KeywordRaw)
                order by name asc
                limit @Limit",
            "user" => @"
                select id, employee_code as code, full_name as name
                from crm_users
                where org_id = @OrgId
                  and is_active = true
                  and (@Keyword is null or unaccent(full_name) ilike unaccent(@Keyword) or employee_code ilike @KeywordRaw)
                order by full_name asc
                limit @Limit",
            _ => throw new BadRequestException("scope type is invalid")
        };

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var keywordParam = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword.Trim()}%";
        var keywordRaw = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword.Trim()}%";
        var items = await connection.QueryAsync<ScopeItemDto>(new CommandDefinition(sql, new
        {
            OrgId = orgId,
            Keyword = keywordParam,
            KeywordRaw = keywordRaw,
            Limit = limit
        }, cancellationToken: cancellationToken));

        return items.ToList();
    }
}
