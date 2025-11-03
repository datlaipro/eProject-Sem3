using FluentValidation;
using VehicleInsurance.Application.Customers.Dtos;

namespace VehicleInsurance.Application.Customers.Validators;

public class CustomerCreateValidator : AbstractValidator<CustomerCreateRequest>
{
    public CustomerCreateValidator()
    {
        Include(new CustomerWriteValidator<CustomerCreateRequest>());
    }
}
