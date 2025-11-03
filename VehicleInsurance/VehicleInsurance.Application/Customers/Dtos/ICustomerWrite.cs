namespace VehicleInsurance.Application.Customers.Dtos;

public interface ICustomerWrite
{
    string FullName { get; }
    string Phone { get; }
    string Address { get; }
}
