using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Validify;

namespace Validify.Tests;

public class GeneratedRegistrationTests
{
  [Test]
  public async Task AddValidifyValidators_RegistersDiscoveredValidator()
  {
    var services = new ServiceCollection();

    services.AddValidifyValidators();

    await using var provider = services.BuildServiceProvider();
    var validator = provider.GetService<IValidator<CreateUser>>();

    await Assert.That(validator).IsTypeOf<CreateUserValidator>();
  }

  [Test]
  public async Task AddValidifyValidators_UsesConfiguredLifetime()
  {
    var services = new ServiceCollection();

    services.AddValidifyValidators(ServiceLifetime.Singleton);

    var descriptor = services.Single(d => d.ServiceType == typeof(IValidator<CreateUser>));
    await Assert.That(descriptor.Lifetime).IsEqualTo(ServiceLifetime.Singleton);
  }

  [Test]
  public async Task AddValidifyValidators_DoesNotOverrideManualRegistration()
  {
    var services = new ServiceCollection();

    // Manual registration first; generated TryAdd must not duplicate or override it.
    services.AddSingleton<IValidator<CreateUser>, CreateUserValidator>();
    services.AddValidifyValidators();

    var count = services.Count(d => d.ServiceType == typeof(IValidator<CreateUser>));
    await Assert.That(count).IsEqualTo(1);
  }
}