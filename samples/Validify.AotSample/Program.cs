// The Validify runtime assembly is aliased (see Validify.AotSample.csproj) to avoid a duplicate
// `ValidifyGeneratedRegistration` clash with this project's own generated copy. The runtime's
// AddValidify / WithValidation extensions are therefore reached through that alias's namespaces.
extern alias validify_runtime;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using validify_runtime::Validify;
using validify_runtime::Validify.Extensions;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default));

builder.Services.AddValidify();            // filter infrastructure
builder.Services.AddValidifyValidators();  // generated — registers CreatePersonValidator

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
