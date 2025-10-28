using System;

namespace CreateCrmWorkItem.Api.Models.Responses;

public record CategoryDto(
    Guid Id,
    string Code,
    string Name,
    int ChildrenCount);
