namespace VehicleInsurance.Domain.Users;


using System.Threading;
using System.Threading.Tasks;


public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default);
    Task<User> AddAsync(User user, CancellationToken ct = default);
}
