namespace CreateCrmWorkItem.Api.Infrastructure.Rbac;

public class PermissionOptions
{
    public required string CrmCategoryRead { get; init; }
    public required string CrmUserRead { get; init; }
    public required string CrmDirectoryRead { get; init; }
    public required string CrmScopeProjectRead { get; init; }
    public required string CrmScopeDepartmentRead { get; init; }
    public required string CrmScopeUserRead { get; init; }
    public required string CrmTemplateSuggestRead { get; init; }
    public required string CrmWorkItemCreate { get; init; }
    public required string CrmWorkItemAdmin { get; init; }
    public required string CrmWorkItemRead { get; init; }
}
