// The Validify runtime assembly is aliased (see Validify.Tests.csproj) to avoid a
// duplicate `ValidifyGeneratedRegistration` clash with this assembly's generated copy.
// Reach the filter type through that alias's namespace.

extern alias validify_runtime;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Filters = validify_runtime::Validify.Filters;

namespace Validify.Tests;

public class ValidationFilterTests
{
  // Pass the single endpoint argument through the strongly-typed Create overload.
  // Routing it via `params object[]` would bind to Create<T>(HttpContext, T) with
  // T = object[], wrapping the whole array as one argument so the filter's
  // OfType<T>() never finds the model. A single typed argument lands in
  // Arguments[0] as its real runtime type, which is what the filter inspects.
  private static EndpointFilterInvocationContext ContextWith(object argument)
  {
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Path = "/users";
    return EndpointFilterInvocationContext.Create(httpContext, argument);
  }

  [Test]
  public async Task InvalidModel_Returns400ProblemDetails()
  {
    var filter = new Filters.ValidationFilter<CreateUser>(new CreateUserValidator());
    var context = ContextWith(new CreateUser { Username = "" });

    var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>("reached-handler"));

    await Assert.That(result)
      .IsTypeOf<Microsoft.AspNetCore.Http.HttpResults.BadRequest<HttpValidationProblemDetails>>();
  }

  [Test]
  public async Task ValidModel_CallsNext()
  {
    var filter = new Filters.ValidationFilter<CreateUser>(new CreateUserValidator());
    var context = ContextWith(new CreateUser { Username = "alice" });

    var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>("reached-handler"));

    await Assert.That(result).IsEqualTo("reached-handler");
  }

  [Test]
  public async Task NoModelArgument_PassesThrough()
  {
    var filter = new Filters.ValidationFilter<CreateUser>(new CreateUserValidator());
    var context = ContextWith("not-a-model");

    var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>("reached-handler"));

    await Assert.That(result).IsEqualTo("reached-handler");
  }
}