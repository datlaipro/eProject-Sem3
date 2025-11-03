using VehicleInsurance.Domain.Entity;

namespace VehicleInsurance.Domain.Customers;

public class Customer
{
    public long Id { get; set; }                 // bigint UNSIGNED
    public long? UserId { get; set; }            // FK -> users.id (nullable, ON DELETE SET NULL)
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? AddressProof { get; set; }
    public DateTime CreatedAt { get; set; }      // timestamp
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public VehicleInsurance.Domain.Users.User? User { get; set; }
    public List<Vehicle> Vehicles { get; set; } = new();
}
