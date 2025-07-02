using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Validify.Filters;

namespace Validify.Extensions;

public static class ValidationEndpointExtensions
{
  public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder)
    where T : class
  {
    return builder.AddEndpointFilter<ValidationFilter<T>>();
  }
}