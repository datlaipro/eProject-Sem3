

using Microsoft.EntityFrameworkCore;
using VehicleInsurance.Application.EmailVerification;
using VehicleInsurance.Domain.EmailVerification;
namespace VehicleInsurance.Infrastructure.EmailVerification;

public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly AppDbContext _db;
    public EmailVerificationTokenRepository(AppDbContext db) => _db = db;

    public async Task SaveAsync(EmailVerificationToken token, CancellationToken ct = default)
    {
        _db.EmailVerificationTokens.Add(token);
        await _db.SaveChangesAsync(ct);
    }

    public Task<EmailVerificationToken?> FindByTokenAsync(string token, CancellationToken ct = default)
    {
        return _db.EmailVerificationTokens
                 .AsNoTracking()
                 .FirstOrDefaultAsync(x => x.Token == token, ct);
    }

    public async Task MarkUsedAsync(long id, CancellationToken ct = default)
    {
        await _db.EmailVerificationTokens
                 .Where(x => x.Id == id)
                 .ExecuteUpdateAsync(s => s.SetProperty(p => p.UsedAtUtc, _ => DateTime.UtcNow), ct);
    }
}
