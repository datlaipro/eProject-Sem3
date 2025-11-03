// CustomerCreateRequest.cs
namespace VehicleInsurance.Application.Customers.Dtos;
public class CustomerCreateRequest : ICustomerWrite
{
    public long UserId { get; set; }        // hoặc Guid tùy schema của bạn
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
