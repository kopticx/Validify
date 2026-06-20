namespace Validify.SourceGenerator.Tests;

public class GeneratorTests
{
  [Test]
  public async Task EmptyCompilation_GeneratesNothing()
  {
    const string source = "namespace Sample { public class Nothing { } }";

    var driver = TestHelper.Run(source);

    await Assert.That(driver.GetRunResult().GeneratedTrees).IsEmpty();
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

  [Test]
  public Task AbstractAndOpenGenericValidators_AreSkipped()
  {
    const string source = """
                          using FluentValidation;

                          namespace Sample.Models { public class Order { public int Id { get; set; } } }

                          namespace Sample.Validators
                          {
                              using Sample.Models;

                              // abstract base — must NOT be registered
                              public abstract class BaseValidator<T> : AbstractValidator<T> { }

                              // open generic — must NOT be registered
                              public sealed class GenericValidator<T> : AbstractValidator<T> { }

                              // concrete — MUST be registered
                              public sealed class OrderValidator : BaseValidator<Order>
                              {
                                  public OrderValidator() => RuleFor(x => x.Id).GreaterThan(0);
                              }
                          }
                          """;

    var driver = TestHelper.Run(source);

    return Verify(driver);
  }

  [Test]
  public Task ValidatorImplementingTwoModels_RegistersBoth()
  {
    const string source = """
                          using FluentValidation;

                          namespace Sample.Models
                          {
                              public class A { public string V { get; set; } = ""; }
                              public class B { public string V { get; set; } = ""; }
                          }

                          namespace Sample.Validators
                          {
                              using Sample.Models;
                              public sealed class DualValidator : AbstractValidator<A>, IValidator<B>
                              {
                                  public DualValidator() => RuleFor(x => x.V).NotEmpty();

                                  FluentValidation.Results.ValidationResult IValidator<B>.Validate(B instance) => new();
                                  System.Threading.Tasks.Task<FluentValidation.Results.ValidationResult> IValidator<B>.ValidateAsync(
                                      B instance, System.Threading.CancellationToken cancellation) => System.Threading.Tasks.Task.FromResult(new FluentValidation.Results.ValidationResult());
                              }
                          }
                          """;

    var driver = TestHelper.Run(source);

    return Verify(driver);
  }
}