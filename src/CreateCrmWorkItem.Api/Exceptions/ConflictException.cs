using System.Net;

namespace CreateCrmWorkItem.Api.Exceptions;

public class ConflictException : ApiException
{
    public ConflictException(string message)
        : base(HttpStatusCode.Conflict, "conflict", message)
    {
    }
}
