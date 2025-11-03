// CustomerUpdateRequest.cs
namespace VehicleInsurance.Application.Customers.Dtos;
public class CustomerUpdateRequest : ICustomerWrite
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
