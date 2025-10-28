using System.Net;

namespace CreateCrmWorkItem.Api.Exceptions;

public class NotFoundException : ApiException
{
    public NotFoundException(string message = "not_found")
        : base(HttpStatusCode.NotFound, "not_found", message)
    {
    }
}
