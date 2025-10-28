using System;
using System.Net;

namespace CreateCrmWorkItem.Api.Exceptions;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string Error { get; }
    public IDictionary<string, string>? Fields { get; }

    public ApiException(HttpStatusCode statusCode, string error, string message, IDictionary<string, string>? fields = null)
        : base(message)
    {
        StatusCode = statusCode;
        Error = error;
        Fields = fields;
    }
}
