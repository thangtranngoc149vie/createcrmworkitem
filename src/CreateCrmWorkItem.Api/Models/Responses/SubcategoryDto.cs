using System;

namespace CreateCrmWorkItem.Api.Models.Responses;

public record SubcategoryDto(
    Guid Id,
    string Code,
    string Name);
