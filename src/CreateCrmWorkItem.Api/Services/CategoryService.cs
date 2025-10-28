using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CreateCrmWorkItem.Api.Infrastructure.Database;
using CreateCrmWorkItem.Api.Models.Common;
using CreateCrmWorkItem.Api.Models.Responses;
using Dapper;

namespace CreateCrmWorkItem.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CategoryService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResult<CategoryDto>> GetRootCategoriesAsync(Guid orgId, int level, string? domain, string? keyword, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var filter = new StringBuilder(" from crm_categories where org_id = @OrgId and level = @Level");
        var parameters = new DynamicParameters();
        parameters.Add("OrgId", orgId);
        parameters.Add("Level", level);

        if (!string.IsNullOrWhiteSpace(domain))
        {
            filter.Append(" and domain = @Domain");
            parameters.Add("Domain", domain);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filter.Append(" and (unaccent(name) ilike unaccent(@Keyword))");
            parameters.Add("Keyword", $"%{keyword.Trim()}%");
        }

        var countSql = "select count(*)" + filter.ToString();
        var itemsSql = "select id, code, name, children_count" + filter + " order by name asc limit @Limit offset @Offset";
        parameters.Add("Limit", pageSize);
        parameters.Add("Offset", (page - 1) * pageSize);

        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var items = await connection.QueryAsync<CategoryDto>(new CommandDefinition(itemsSql, parameters, cancellationToken: cancellationToken));

        return new PagedResult<CategoryDto>(items.ToList(), total, page, pageSize);
    }

    public async Task<IReadOnlyList<SubcategoryDto>> GetSubcategoriesAsync(Guid orgId, Guid categoryId, string? keyword, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        parameters.Add("OrgId", orgId);
        parameters.Add("CategoryId", categoryId);

        var sql = new StringBuilder(@"
            select id, code, name
            from crm_subcategories
            where org_id = @OrgId and parent_category_id = @CategoryId");

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            sql.Append(" and (unaccent(name) ilike unaccent(@Keyword))");
            parameters.Add("Keyword", $"%{keyword.Trim()}%");
        }

        sql.Append(" order by name asc");

        var items = await connection.QueryAsync<SubcategoryDto>(new CommandDefinition(sql.ToString(), parameters, cancellationToken: cancellationToken));
        return items.ToList();
    }
}
