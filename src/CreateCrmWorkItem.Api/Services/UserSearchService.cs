using System;
using System.Collections.Generic;
using System.Linq;
using CreateCrmWorkItem.Api.Infrastructure.Database;
using CreateCrmWorkItem.Api.Models.Responses;
using Dapper;

namespace CreateCrmWorkItem.Api.Services;

public class UserSearchService : IUserSearchService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserSearchService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> SearchAsync(Guid orgId, string? keyword, int limit, CancellationToken cancellationToken)
    {
        if (limit <= 0) limit = 10;
        if (limit > 50) limit = 50;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        const string sql = @"
            select id, full_name as FullName, email
            from crm_users
            where org_id = @OrgId
              and is_active = true
              and (@Keyword is null or unaccent(full_name) ilike unaccent(@Keyword) or unaccent(email) ilike unaccent(@Keyword))
            order by full_name asc
            limit @Limit";

        var users = await connection.QueryAsync<UserSummaryDto>(new CommandDefinition(sql, new
        {
            OrgId = orgId,
            Keyword = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword.Trim()}%",
            Limit = limit
        }, cancellationToken: cancellationToken));

        return users.ToList();
    }
}
