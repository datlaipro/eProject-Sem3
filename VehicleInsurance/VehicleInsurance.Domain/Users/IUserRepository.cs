namespace VehicleInsurance.Domain.Users;

using System.Threading;
using System.Threading.Tasks;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> FindByIdAsync(long id, CancellationToken ct = default); // 👈 THÊM DÒNG NÀY
    Task<User> AddAsync(User user, CancellationToken ct = default);
    Task<List<string>> GetUserRolesAsync(long userId, CancellationToken ct = default);
    Task<List<string>> GetUserPermissionsAsync(long userId, CancellationToken ct = default);
}
