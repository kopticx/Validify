namespace Validify.SourceGenerator.Tests;

public class GeneratorTests
{
  [Test]
  public Task EmptyCompilation_GeneratesEmptyRegistrationMethod()
  {
    const string source = "namespace Sample { public class Nothing { } }";

    var driver = TestHelper.Run(source);

    return Verify(driver);
  }

  [Test]
  public Task LocalValidator_IsRegistered()
  {
    const string source = """
      using FluentValidation;

      namespace Sample.Models { public class User { public string Name { get; set; } = ""; } }

      namespace Sample.Validators
      {
          using Sample.Models;
          public sealed class UserValidator : AbstractValidator<User>
          {
              public UserValidator() => RuleFor(x => x.Name).NotEmpty();
          }
      }
      """;

    var driver = TestHelper.Run(source);

    return Verify(driver);
  }
}