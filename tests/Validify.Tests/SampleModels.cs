using FluentValidation;

namespace Validify.Tests;

public sealed class CreateUser
{
  public string Username { get; set; } = "";
}

public sealed class CreateUserValidator : AbstractValidator<CreateUser>
{
  public CreateUserValidator() =>
    RuleFor(x => x.Username).NotEmpty().MaximumLength(20);
}