using System.Text.Json.Serialization;
using FluentValidation;
using Validify;
using Validify.Extensions;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default));

builder.Services.AddValidify();            // filter infrastructure
builder.Services.AddValidifyValidators();  // generated — registers this project's validators

var app = builder.Build();

app.MapPost("/people", (CreatePerson person) => Results.Ok(person))
   .WithValidation<CreatePerson>();

app.Run();

public sealed class CreatePerson
{
    public string Name { get; set; } = "";
}

public sealed class CreatePersonValidator : AbstractValidator<CreatePerson>
{
    public CreatePersonValidator() => RuleFor(x => x.Name).NotEmpty();
}

[JsonSerializable(typeof(CreatePerson))]
[JsonSerializable(typeof(HttpValidationProblemDetails))]
public partial class AppJsonContext : JsonSerializerContext;
