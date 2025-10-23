// VehicleInsurance.Domain/Roles/Role.cs
namespace VehicleInsurance.Domain.Roles
{
    public class Role
    {
        public int RoleId { get; set; }             // INT (AI)
        public string Name { get; set; } = null!;   // UNIQUE, <= 50
        public string? Description { get; set; }    // <= 200
        public DateTime CreatedAt { get; set; }     // map -> CreatedAt
        public DateTime UpdatedAt { get; set; }     // map -> UpdatedAt
    }
}
