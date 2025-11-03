using System.ComponentModel.DataAnnotations;

namespace VehicleInsurance.Application.Auth;

public class RegisterRequest
{
    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; } = default!;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = default!;

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = default!;
}

public record LoginRequest(
    [property: Required] string EmailOrUsername,
    [property: Required] string Password
);

public record AuthResult(
    long UserId,
    string Username,
    string Email,
    string Role,                       // Giữ lại để tương thích cũ
    IEnumerable<string>? Roles = null, // Nếu người dùng có nhiều vai trò
    IEnumerable<string>? Permissions = null // Nếu có hệ thống quyền chi tiết
);