using System.Net;
using System.Text.Json;
using FluentValidation;
using VehicleInsurance.Application.Common.Errors;
using VehicleInsurance.Application.Common.Exceptions;

namespace VehicleInsurance.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteErrorAsync(context, ex);
        }
    }

    private static Task WriteErrorAsync(HttpContext ctx, Exception ex)
    {
        var traceId = ctx.TraceIdentifier;

        // Mặc định 500
        var status = HttpStatusCode.InternalServerError;
        var code = ErrorCodes.ServerError;
        string message = "Internal server error";
        object? errors = null;

       switch (ex)
{
    case AppException ax:
        status = ax.Status;
        code = ax.Code;
        message = ax.Message;

        // Nếu là 429 và có Retry-After → set header
        if (ax is RateLimitAppException rl && rl.RetryAfterSeconds.HasValue)
            ctx.Response.Headers["Retry-After"] = rl.RetryAfterSeconds.Value.ToString();

        break;

    case ValidationException ve:
        status = HttpStatusCode.UnprocessableContent;
        code = ErrorCodes.Validation;
        message = "Validation failed";
        errors = ve.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        break;

    case UnauthorizedAccessException:
        status = HttpStatusCode.Unauthorized;
        code = ErrorCodes.Unauthorized;
        message = "Unauthorized";
        break;

    default:
        // giữ mặc định 500
        break;
}

        var payload = new
        {
            error = new { code, message, errors },
            traceId
        };

        ctx.Response.StatusCode = (int)status;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOpts));
    }
}
