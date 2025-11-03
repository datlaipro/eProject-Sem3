namespace VehicleInsurance.Application.EmailVerification;

public interface IEmailVerificationService
{
    Task SendVerificationAsync(long userId, string email, string username, CancellationToken ct = default);
     Task SendVerificationAsync(long? userId = null, string? email = null, string? username = null, CancellationToken ct = default);
    Task<bool> VerifyAsync(string token, CancellationToken ct = default);
}
