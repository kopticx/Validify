using System.Text.Json.Serialization;
using FluentValidation;
using Validify;
using Validify.AotSample.Domain;
using Validify.Extensions;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default));

builder.Services.AddValidify();            // filter infrastructure
builder.Services.AddValidifyValidators();  // generated — registers CreatePersonValidator

var app = builder.Build();

app.MapPost("/people", (CreatePerson person) => Results.Ok(person))
   .WithValidation<CreatePerson>();

// Company / CompanyValidator live in a SEPARATE assembly (Validify.AotSample.Domain). The single
// AddValidifyValidators() call above must discover and register CompanyValidator from that
// referenced assembly with no manual registration and no alias.
app.MapPost("/companies", (Company company) => Results.Ok(company))
   .WithValidation<Company>();

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
[JsonSerializable(typeof(Company))]
[JsonSerializable(typeof(HttpValidationProblemDetails))]
public partial class AppJsonContext : JsonSerializerContext;
