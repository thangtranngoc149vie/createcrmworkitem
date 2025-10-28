using System;
using System.Collections.Generic;

namespace CreateCrmWorkItem.Api.Models.Responses;

public class WorkItemDetailResponse
{
    public Guid Id { get; init; }
    public string RefCode { get; init; } = string.Empty;
    public string RefType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public CategorySummary? Category { get; init; }
    public SubcategorySummary? Subcategory { get; init; }
    public UserSummary? Assignee { get; init; }
    public IReadOnlyCollection<UserSummary> Watchers { get; init; } = Array.Empty<UserSummary>();
    public DateTimeOffset? DueAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public record CategorySummary(Guid Id, string Name);
    public record SubcategorySummary(Guid Id, string Name);
    public record UserSummary(Guid Id, string FullName);
}
