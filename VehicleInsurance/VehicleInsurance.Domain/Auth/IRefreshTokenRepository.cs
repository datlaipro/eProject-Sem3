using System.Threading;
using System.Threading.Tasks;

namespace VehicleInsurance.Domain.Auth;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> FindValidAsync(ulong userId, string tokenHash, CancellationToken ct = default);
    Task RevokeAsync(RefreshToken token, string? replacedByHash = null, CancellationToken ct = default);
    Task RevokeFamilyAsync(ulong userId, string tokenFamily, CancellationToken ct = default); // optional
}
