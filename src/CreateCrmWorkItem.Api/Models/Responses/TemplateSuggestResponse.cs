using System;

namespace CreateCrmWorkItem.Api.Models.Responses;

public record TemplateSuggestResponse(
    string TemplateCode,
    TemplateSuggestAssignment? Assignment,
    TemplateSuggestScope? Scope,
    int? DueInDays);

public record TemplateSuggestAssignment(string Mode, Guid? AssigneeId, string? Display);

public record TemplateSuggestScope(string? Type, Guid? EntityId, string? Display);
