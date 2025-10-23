

using Microsoft.EntityFrameworkCore;
using VehicleInsurance.Domain.Users;

namespace VehicleInsurance.Infrastructure.Users;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }
}
