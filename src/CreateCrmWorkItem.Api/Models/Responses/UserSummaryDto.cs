using System;

namespace CreateCrmWorkItem.Api.Models.Responses;

public record UserSummaryDto(
    Guid Id,
    string FullName,
    string Email);
