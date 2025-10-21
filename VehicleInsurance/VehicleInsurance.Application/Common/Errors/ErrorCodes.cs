namespace VehicleInsurance.Application.Common.Errors;

public static class ErrorCodes
{
    public const string EmailExists = "USER_EMAIL_EXISTS";
    public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string Unauthorized = "GEN_UNAUTHORIZED";
    public const string Forbidden = "GEN_FORBIDDEN";
    public const string NotFound = "GEN_NOT_FOUND";
    public const string Conflict = "GEN_CONFLICT";
    public const string Validation = "GEN_VALIDATION_ERROR";
    public const string BadRequest = "GEN_BAD_REQUEST";
    public const string ServerError = "GEN_INTERNAL_SERVER_ERROR";
}
