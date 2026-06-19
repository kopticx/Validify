using Microsoft.Extensions.DependencyInjection;
using Validify.Filters;

namespace Validify;

public static class ValidifyRegistration
{
  public static IServiceCollection AddValidify(this IServiceCollection services)
  {
    services.AddScoped(typeof(ValidationFilter<>));
    
    return services;
  }
}