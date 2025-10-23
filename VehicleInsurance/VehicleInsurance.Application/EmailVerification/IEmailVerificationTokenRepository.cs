

using VehicleInsurance.Domain.EmailVerification;
namespace VehicleInsurance.Application.EmailVerification;

public interface IEmailVerificationTokenRepository
{
    Task SaveAsync(EmailVerificationToken token, CancellationToken ct = default);
    Task<EmailVerificationToken?> FindByTokenAsync(string token, CancellationToken ct = default);
    Task MarkUsedAsync(long id, CancellationToken ct = default);
}
