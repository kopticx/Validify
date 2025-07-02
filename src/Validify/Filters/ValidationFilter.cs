using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Validify.Filters;

public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
  public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
  {
    var model = context.Arguments.OfType<T>().FirstOrDefault();

    if (model is null) 
      return await next(context);
    
    var result = await validator.ValidateAsync(model);
    
    if (result.IsValid) 
      return await next(context);
    
    var problemDetails = new HttpValidationProblemDetails
    {
      Status = StatusCodes.Status400BadRequest,
      Title = "Validation failed",
      Detail = "One or more validation errors occurred.",
      Instance = context.HttpContext.Request.Path,
      Errors = result.ToDictionary()
    };
        
    return Results.BadRequest(problemDetails);
  }
}