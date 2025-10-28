using System.Collections.Generic;
using System.Net;

namespace CreateCrmWorkItem.Api.Exceptions;

public class BadRequestException : ApiException
{
    public BadRequestException(string message, IDictionary<string, string>? fields = null)
        : base(HttpStatusCode.BadRequest, "bad_request", message, fields)
    {
    }
}
