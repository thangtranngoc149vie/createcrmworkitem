using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CreateCrmWorkItem.Api.Models.Requests;

public class WorkItemCreateRequest
{
    [Required]
    [RegularExpression("^(request|ticket|approval)$", ErrorMessage = "type must be request, ticket, or approval")]
    public string Type { get; set; } = string.Empty;

    public string? Domain { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? SubcategoryId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [RegularExpression("^(low|medium|high)$", ErrorMessage = "priority must be low, medium, or high")]
    public string Priority { get; set; } = "medium";

    public DateTimeOffset? DueAt { get; set; }

    public ScopeDto? Scope { get; set; }

    public Guid? AssigneeId { get; set; }

    public IReadOnlyCollection<Guid> WatcherIds { get; set; } = Array.Empty<Guid>();

    public class ScopeDto
    {
        [Required]
        [RegularExpression("^(project|department|user)$", ErrorMessage = "scope.type must be project, department, or user")]
        public string Type { get; set; } = string.Empty;

        [Required]
        public Guid EntityId { get; set; }
    }
}
