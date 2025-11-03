// File: VehicleInsurance.Application/Common/Exceptions/AppExceptions.cs
using System.Net;

namespace VehicleInsurance.Domain.Common.Exceptions
{
    // Base: có Code + Status để middleware đọc
    public abstract class AppException : Exception
    {
        public string Code { get; }
        public HttpStatusCode Status { get; }

        protected AppException(string code, string message, HttpStatusCode status)
            : base(message)
        {
            Code = code;
            Status = status;
        }
    }

    // 400 Bad Request
    public class BadRequestAppException : AppException
    {
        public BadRequestAppException(string message, string code = "GEN_BAD_REQUEST")
            : base(code, message, HttpStatusCode.BadRequest) { }
    }

    // 401 Unauthorized
    public class UnauthorizedAppException : AppException
    {
        public UnauthorizedAppException(string message, string code = "GEN_UNAUTHORIZED")
            : base(code, message, HttpStatusCode.Unauthorized) { }
    }

    // 403 Forbidden
    public class ForbiddenAppException : AppException
    {
        public ForbiddenAppException(string message, string code = "GEN_FORBIDDEN")
            : base(code, message, HttpStatusCode.Forbidden) { }
    }

    // 404 Not Found
    public class NotFoundException : AppException
    {
        public NotFoundException(string message, string code = "GEN_NOT_FOUND")
            : base(code, message, HttpStatusCode.NotFound) { }
    }

    // 409 Conflict
    public class ConflictException : AppException
    {
        public ConflictException(string message, string code = "GEN_CONFLICT")
            : base(code, message, HttpStatusCode.Conflict) { }
    }

    // 410 Gone
    public class GoneAppException : AppException
    {
        public GoneAppException(string message, string code = "GEN_GONE")
            : base(code, message, HttpStatusCode.Gone) { }
    }

    // 429 Too Many Requests
    public class RateLimitAppException : AppException
    {
        public int? RetryAfterSeconds { get; }
        public RateLimitAppException(string message, int? retryAfterSeconds = null, string code = "GEN_RATE_LIMIT")
            : base(code, message, (HttpStatusCode)429)
        {
            RetryAfterSeconds = retryAfterSeconds;
        }
    }

    // Auth-specific
    public class InvalidLoginException : AppException
    {
        public InvalidLoginException(string message, string code = "GEN_INVALID_LOGIN")
            : base(code, message, HttpStatusCode.Unauthorized) { }
    }

    public class InvalidRefreshTokenException : AppException
    {
        public InvalidRefreshTokenException(string message, string code = "GEN_INVALID_REFRESH_TOKEN")
            : base(code, message, HttpStatusCode.Unauthorized) { }
    }
}
