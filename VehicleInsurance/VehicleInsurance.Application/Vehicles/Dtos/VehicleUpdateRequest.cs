namespace VehicleInsurance.Application.Vehicles.Dtos;

public class VehicleUpdateRequest
{
    public string? Name { get; set; }
    public string? OwnerName { get; set; }
    public string? Model { get; set; }
    public string? Version { get; set; }
    public int? SeatCount { get; set; }     // ✅ Thêm số chỗ ngồi
    public string? BodyNumber { get; set; }
    public string? EngineNumber { get; set; }
    public string? VehicleNumber { get; set; }
}
