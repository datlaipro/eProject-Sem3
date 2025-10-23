// VehicleInsurance.Domain/Roles/UserRole.cs
namespace VehicleInsurance.Domain.Roles
{
    public class UserRole
    {
        // Chọn kiểu giống với User.Id (thường là long)
        public long UserId { get; set; } // BIGINT UNSIGNED -> thường dùng long ở C#
        public int RoleId { get; set; }
    }
}
