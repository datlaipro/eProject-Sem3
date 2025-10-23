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
    public const string Gone      = "GEN_GONE";
    public const string RateLimit = "GEN_RATE_LIMIT";

    // Email verify use-cases
    public const string VerifyCriteriaMissing = "VERIFY_CRITERIA_MISSING";
    public const string VerifyUserNotFound    = "VERIFY_USER_NOT_FOUND";
    public const string VerifyMismatch        = "VERIFY_IDENTITY_MISMATCH";
    public const string VerifyAlreadyDone     = "EMAIL_ALREADY_VERIFIED";
    public const string VerifyTokenInvalid    = "TOKEN_INVALID_OR_EXPIRED";
    public const string VerifyTooSoon         = "VERIFY_TOO_SOON";
}
