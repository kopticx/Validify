# Kopticx.Validify

[![NuGet](https://img.shields.io/nuget/v/Kopticx.Validify?style=flat-square)](https://www.nuget.org/packages/Kopticx.Validify)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue?style=flat-square)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

**Validify** is a lightweight validation filter for ASP.NET Core Minimal APIs powered by [FluentValidation](https://fluentvalidation.net/). It wires model validation into your endpoint pipeline with one line per endpoint — and a **source generator registers all your validators for you**, with **zero runtime reflection** so the whole thing stays fully **Native AOT** compatible.

---

## ✨ Features

- ✅ Declarative model validation for Minimal APIs via `.WithValidation<T>()`
- 🔄 Built on FluentValidation — full rule set: conditional, cross-property, async, custom messages
- 🤖 **Source-generated auto-registration** — one `AddValidifyValidators()` call registers every validator; no manual `AddSingleton` per model
- 🏛️ **Multi-assembly discovery** — finds validators in referenced projects (Clean Architecture / N-layer), not just the startup project
- ⚙️ **100% Native AOT compatible** — discovery happens at compile time, no runtime reflection
- 🛡️ Stops invalid requests before they reach your handlers, with an RFC 9457 `HttpValidationProblemDetails`
- 🧩 Plug-and-play with `Microsoft.Extensions.DependencyInjection`

---

## 🚀 Installation

```bash
dotnet add package Kopticx.Validify
```

---

## 🛠️ Setup

### 1. Register Validify

```csharp
builder.Services.AddValidify();            // the validation filter
builder.Services.AddValidifyValidators();  // source-generated — registers all your validators
```

### 2. Write FluentValidation validators

```csharp
public sealed class PostUserModelValidator : AbstractValidator<PostUserModel>
{
    public PostUserModelValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(255).WithMessage("Must be less than 255 characters");
    }
}
```

### 3. Apply validation to an endpoint

```csharp
app.MapPost("/users", PostUser)
   .WithValidation<PostUserModel>();
```

If the model is invalid, the filter short-circuits with a `400 Bad Request` carrying an `HttpValidationProblemDetails` (RFC 9457) — your handler never runs.

---

## 🤖 Auto-registration (source generator)

`AddValidifyValidators()` is generated at **compile time** into your startup assembly. It discovers every `AbstractValidator<T>` and registers `IValidator<T>` → your concrete validator — no reflection, no per-model boilerplate.

It registers with `TryAdd`, so it **coexists with manual registration**: anything you register by hand takes precedence, and the generator fills in the rest.

```csharp
// Still valid — a manual registration wins over the generated one:
services.AddSingleton<IValidator<PostUserModel>, PostUserModelValidator>();
```

The default lifetime is `Scoped` (matching FluentValidation's own default); override it if needed:

```csharp
builder.Services.AddValidifyValidators(ServiceLifetime.Singleton);
```

---

## 🏛️ Clean Architecture / N-layer (multi-assembly)

Validators rarely live in the web project. Validify's generator scans **referenced assemblies** too, so a single `AddValidifyValidators()` in your API project registers public validators defined in `Domain`, `Application`, or any referenced layer:

```
MyApi            →  AddValidify(); AddValidifyValidators();
 └─ Application   →  public CreateOrderValidator : AbstractValidator<CreateOrder>
     └─ Domain    →  public CompanyValidator    : AbstractValidator<Company>
```

Every validator above is discovered and registered at compile time by that one call. (Validators must be `public` to be visible across assembly boundaries — the FluentValidation convention anyway.)

---

## ⚙️ Native AOT

Validify is **100% AOT-compatible**: discovery and registration are resolved at compile time with no runtime reflection. Publish your API as a native executable:

```xml
<PublishAot>true</PublishAot>
```

So the `400` response can be serialized under source-generated `System.Text.Json`, add `HttpValidationProblemDetails` to your `JsonSerializerContext`:

```csharp
[JsonSerializable(typeof(HttpValidationProblemDetails))]
public partial class AppJsonContext : JsonSerializerContext;
```

The repo's `samples/Validify.AotSample` is a Minimal API published with `PublishAot` that registers a validator from a **separate** assembly — proving the multi-assembly + AOT path end to end.

---

## 🤔 Validify vs .NET 10 native validation

.NET 10 introduced built-in Minimal API validation (`AddValidation()` plus a source generator over **DataAnnotations** attributes) — a great fit when DataAnnotations covers your needs. Validify is for teams who prefer **FluentValidation**:

| | **Validify** | **.NET 10 native validation** |
|---|---|---|
| Validation engine | FluentValidation | DataAnnotations |
| Where rules live | Dedicated validator classes, decoupled from the model | Attributes on the model |
| Expressiveness | Fluent API: conditional, cross-property, async, custom messages | Attribute set; custom via `IValidatableObject` / custom attributes |
| Validator testability | Plain classes, unit-testable in isolation | Coupled to the model |
| Registration | One source-generated `AddValidifyValidators()` (or manual) | `AddValidation()` + source-gen over annotated types |
| Multi-project discovery | ✅ public validators across referenced assemblies | Per annotated type |
| Native AOT | ✅ | ✅ |
| Minimum target | .NET 8 | .NET 10 |

Both are source-generated and AOT-friendly — the real decision is **FluentValidation vs DataAnnotations**.

---

## 📝 License

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)

---

## 📄 Third-party licenses

Validify uses [FluentValidation](https://github.com/FluentValidation/FluentValidation), licensed under the [Apache 2.0 License](http://www.apache.org/licenses/LICENSE-2.0).

See `NOTICE` file for more information.