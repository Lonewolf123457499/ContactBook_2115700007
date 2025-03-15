using FluentValidation;
using ModelLayer.DTO;
 // Ensure this namespace matches where your DTO is stored

public class AddressBookEntryDTOValidator : AbstractValidator<AddressBookEntryDTO>
{
    public AddressBookEntryDTOValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\d{10}$").WithMessage("Phone number must be exactly 10 digits");

        RuleFor(x => x.EmailAddress)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters");
    }
}
