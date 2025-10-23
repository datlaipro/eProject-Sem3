


using Microsoft.EntityFrameworkCore;
using VehicleInsurance.Domain.Auth;

namespace VehicleInsurance.Infrastructure.Auth;
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;
    public RefreshTokenRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync(ct);
    }

    public Task<RefreshToken?> FindValidAsync(ulong userId, string tokenHash, CancellationToken ct = default)
        => _db.RefreshTokens.FirstOrDefaultAsync(x =>
               x.UserId == userId && x.TokenHash == tokenHash && !x.Revoked && x.ExpiresAt > DateTime.UtcNow, ct);

    public async Task RevokeAsync(RefreshToken token, string? replacedByHash = null, CancellationToken ct = default)
    {
        token.Revoked = true;
        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByTokenHash = replacedByHash;
        _db.RefreshTokens.Update(token);
        await _db.SaveChangesAsync(ct);
    }

public async Task<int> RevokeFamilyAsync(ulong userId, string family, CancellationToken ct = default)
{
    var now = DateTime.UtcNow;

    return await _db.RefreshTokens
        .Where(r => r.UserId == userId
                    && r.TokenFamily == family
                    && !r.Revoked
                    && r.ExpiresAt > now)
        .ExecuteUpdateAsync(s => s
            .SetProperty(r => r.Revoked, true)
            .SetProperty(r => r.RevokedAt, now)     // nếu có cột
            .SetProperty(r => r.UpdatedAt, now),     // nếu có cột
             ct); // nếu có cột
}

}
