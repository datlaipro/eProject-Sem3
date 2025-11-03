using System.Threading;
using System.Threading.Tasks;

namespace VehicleInsurance.Domain.Auth;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> FindValidAsync(long userId, string tokenHash, CancellationToken ct = default);
    Task RevokeAsync(RefreshToken token, string? replacedByHash = null, CancellationToken ct = default);
        Task<int> RevokeFamilyAsync(long userId, string family, CancellationToken ct = default);

}
