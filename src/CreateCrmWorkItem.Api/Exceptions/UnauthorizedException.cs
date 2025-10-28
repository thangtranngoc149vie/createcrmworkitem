using System.Net;

namespace CreateCrmWorkItem.Api.Exceptions;

public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message = "unauthorized")
        : base(HttpStatusCode.Unauthorized, "unauthorized", message)
    {
    }
}
