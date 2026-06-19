using FluentValidation;

namespace Validify.AotSample.Domain;

public sealed class Company
{
    public string Name { get; set; } = "";
}

public sealed class CompanyValidator : AbstractValidator<Company>
{
    public CompanyValidator() => RuleFor(x => x.Name).NotEmpty().WithMessage("Company name is required");
}
