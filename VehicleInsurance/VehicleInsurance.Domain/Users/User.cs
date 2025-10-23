using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleInsurance.Domain.Users
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string Role { get; set; } = "CUSTOMER"; // CUSTOMER/EMPLOYEE/ADMIN
        public bool Active { get; set; } = true;

        // NEW: đã xác minh email?
        [Column("email_confirmed")]               // khớp tên cột DB
        public bool EmailConfirmed { get; set; } = false;
    }
}
