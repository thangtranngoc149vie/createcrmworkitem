using System.Net;
using System.Text.Json;
using CreateCrmWorkItem.Api.Exceptions;
using CreateCrmWorkItem.Api.Models.Responses;

namespace CreateCrmWorkItem.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException apiEx)
        {
            await WriteErrorAsync(context, apiEx.StatusCode, apiEx.Error, apiEx.Message, apiEx.Fields);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "server_error", "internal server error");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode status, string error, string message, IDictionary<string, string>? fields = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        var response = new ErrorResponse(error, message, fields, context.TraceIdentifier);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
