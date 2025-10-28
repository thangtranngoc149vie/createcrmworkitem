using System.Collections.Generic;

namespace CreateCrmWorkItem.Api.Models.Responses;

public record ErrorResponse(
    string Error,
    string Message,
    IDictionary<string, string>? Fields,
    string? TraceId);
