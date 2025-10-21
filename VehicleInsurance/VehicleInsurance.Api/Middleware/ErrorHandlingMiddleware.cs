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
        object? errors = null; // cho Validation

        switch (ex)
        {
            // App-defined
            case UnauthorizedAppException ua:
                status = HttpStatusCode.Unauthorized;
                code = ua.Code; message = ua.Message;
                break;

            case ForbiddenAppException fb:
                status = HttpStatusCode.Forbidden;
                code = fb.Code; message = fb.Message;
                break;

            case NotFoundException nf:
                status = HttpStatusCode.NotFound;
                code = nf.Code; message = nf.Message;
                break;

            case ConflictException cf:
                status = HttpStatusCode.Conflict;
                code = cf.Code; message = cf.Message;
                break;

            case BadRequestAppException br:
                status = HttpStatusCode.BadRequest;
                code = br.Code; message = br.Message;
                break;

            // FluentValidation
            case ValidationException ve:
                status = HttpStatusCode.UnprocessableContent; // 422
                code = ErrorCodes.Validation;
                message = "Validation failed";
                errors = ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                break;

            // Thêm xử lý cho các exception tùy chỉnh
            case InvalidLoginException ilex:
                status = HttpStatusCode.Unauthorized;
                code = ilex.Code;
                message = ilex.Message;
                break;

            case InvalidRefreshTokenException irtex:
                status = HttpStatusCode.Unauthorized;
                code = irtex.Code;
                message = irtex.Message;
                break;

            // Một số .NET/EF common dễ gặp
            case UnauthorizedAccessException:
                status = HttpStatusCode.Unauthorized;
                code = ErrorCodes.Unauthorized; message = "Unauthorized";
                break;

            default:
                // Giữ 500 + message ngắn gọn; có thể log ex ở đây
                break;
        }

        var payload = new
        {
            error = new
            {
                code,
                message,
                errors,
            },
            traceId
        };

        ctx.Response.StatusCode = (int)status;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOpts));
    }
}
