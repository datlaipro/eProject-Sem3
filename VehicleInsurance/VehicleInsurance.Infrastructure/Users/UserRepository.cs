using Microsoft.EntityFrameworkCore;
using VehicleInsurance.Domain.Users;
using VehicleInsurance.Infrastructure.Data;
namespace VehicleInsurance.Infrastructure.Users;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
        => await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default)
        => await _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    // 🟢 Lấy danh sách role của user
    public async Task<List<string>> GetUserRolesAsync(long userId, CancellationToken ct = default)
    {
        return await (
            from ur in _db.UserRoles
            join r in _db.Roles on ur.RoleId equals r.RoleId   // 👈 đổi sang RoleId
            where ur.UserId == userId
            select r.Name
        ).ToListAsync(ct);
    }

    // 🟢 Vì bạn chưa có bảng role_permissions, nên hàm này trả về danh sách rỗng
    public Task<List<string>> GetUserPermissionsAsync(long userId, CancellationToken ct = default)
        => Task.FromResult(new List<string>());
        public async Task<User?> FindByIdAsync(long id, CancellationToken ct = default)
    => await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

}
