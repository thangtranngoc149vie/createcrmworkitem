using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CreateCrmWorkItem.Api.Infrastructure.Database;
using Dapper;

namespace CreateCrmWorkItem.Api.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> HasAnyPermissionAsync(Guid userId, IEnumerable<string> permissions, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var permissionArray = permissions.Distinct().ToArray();
        if (permissionArray.Length == 0)
        {
            return true;
        }

        const string sql = @"
            select exists (
                select 1
                from crm_user_permission_view
                where user_id = @UserId
                  and permission = any(@Permissions)
            );";

        var result = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { UserId = userId, Permissions = permissionArray },
            cancellationToken: cancellationToken));

        return result;
    }
}
