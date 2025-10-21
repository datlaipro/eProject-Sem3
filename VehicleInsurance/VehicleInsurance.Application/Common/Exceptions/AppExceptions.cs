
namespace VehicleInsurance.Application.Common.Exceptions;

// Base có 'Code' để middleware đọc và map
public abstract class AppException : Exception
{
    public string Code { get; }
    protected AppException(string code, string message) : base(message) => Code = code;
}

public class ConflictException : AppException
{
    public ConflictException(string message, string code = "GEN_CONFLICT") : base(code, message) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message, string code = "GEN_NOT_FOUND") : base(code, message) { }
}

public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message, string code = "GEN_UNAUTHORIZED") : base(code, message) { }
}

public class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message, string code = "GEN_FORBIDDEN") : base(code, message) { }
}

public class BadRequestAppException : AppException
{
    public BadRequestAppException(string message, string code = "GEN_BAD_REQUEST") : base(code, message) { }
}
// Lỗi tài khoản hoặc mật khẩu sai
public class InvalidLoginException : AppException
{
    public InvalidLoginException(string message, string code = "GEN_INVALID_LOGIN") : base(code, message) { }
}

public class InvalidRefreshTokenException : AppException
{
    public InvalidRefreshTokenException(string message, string code = "GEN_INVALID_REFRESH_TOKEN") : base(code, message) { }
}
