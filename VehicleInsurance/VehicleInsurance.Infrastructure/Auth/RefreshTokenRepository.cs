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

    public async Task RevokeFamilyAsync(ulong userId, string tokenFamily, CancellationToken ct = default)
    {
        var list = await _db.RefreshTokens.Where(r => r.UserId == userId && r.TokenFamily == tokenFamily && !r.Revoked).ToListAsync(ct);
        foreach (var t in list) { t.Revoked = true; t.RevokedAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync(ct);
    }
}
