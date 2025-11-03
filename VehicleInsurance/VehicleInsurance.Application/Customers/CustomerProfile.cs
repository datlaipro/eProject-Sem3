using AutoMapper;
using VehicleInsurance.Application.Customers.Dtos;
using VehicleInsurance.Domain.Customers;

namespace VehicleInsurance.Application.Customers;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<CustomerCreateRequest, Customer>();
        CreateMap<CustomerUpdateRequest, Customer>();
    }
}
