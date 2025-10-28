using System;

namespace CreateCrmWorkItem.Api.Models.Responses;

public record ScopeItemDto(
    Guid Id,
    string? Code,
    string Name);
