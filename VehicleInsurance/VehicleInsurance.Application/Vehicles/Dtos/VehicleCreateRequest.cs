
namespace VehicleInsurance.Application.Vehicles.Dtos;
public class VehicleCreateRequest
{
    public long? CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string BodyNumber { get; set; } = string.Empty;
    public string EngineNumber { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
}
