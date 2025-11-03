using FluentValidation;
using VehicleInsurance.Application.Customers.Dtos;

namespace VehicleInsurance.Application.Customers.Validators;

public class CustomerWriteValidator<T> : AbstractValidator<T>
    where T : VehicleInsurance.Application.Customers.Dtos.ICustomerWrite // <- CHỐT namespace
{
    public CustomerWriteValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^[0-9]{9,11}$").WithMessage("Invalid phone number");
        RuleFor(x => x.Address).NotEmpty().MaximumLength(255);
    }
}
