using System;

namespace CreateCrmWorkItem.Api.Models.Responses;

public record WorkItemCreatedResponse(
    Guid Id,
    string RefCode,
    string Status,
    DateTimeOffset CreatedAt);
