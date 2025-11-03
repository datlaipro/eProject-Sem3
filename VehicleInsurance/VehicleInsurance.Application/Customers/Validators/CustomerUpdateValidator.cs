using FluentValidation;
using VehicleInsurance.Application.Customers.Dtos;
namespace VehicleInsurance.Application.Customers.Validators;

public class CustomerUpdateValidator : AbstractValidator<CustomerUpdateRequest>
{
    public CustomerUpdateValidator()
    {
        Include(new CustomerWriteValidator<CustomerUpdateRequest>());
    }
}
