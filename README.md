# Kopticx.Validify

[![NuGet](https://img.shields.io/nuget/v/Kopticx.Validify?style=flat-square)](https://www.nuget.org/packages/Kopticx.Validify)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue?style=flat-square)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

**Validify** is a lightweight validation filter for ASP.NET Core Minimal APIs that uses [FluentValidation](https://fluentvalidation.net/) under the hood. It allows you to inject model validation seamlessly into your API pipeline using a simple and expressive syntax.

---

## ✨ Features

- ✅ Declarative model validation for Minimal APIs
- 🔄 Integrates with FluentValidation validators
- 🧩 Plug-and-play with `Microsoft.Extensions.DependencyInjection`
- 🛡️ Prevents invalid requests from reaching your handlers
- 📦 Available as a NuGet package
- ⚙️ Compatible with Native AOT

---

## 🚀 Installation

Install from NuGet:

```bash
dotnet add package Validify
```

---

## ⚙️ How it works

Validify registers a filter that intercepts requests before they hit your endpoint. If the request model is invalid, a `400 Bad Request` is returned with validation details.

### Example usage:

```csharp
companiesApi.MapPost("/", PostUser)
    .WithValidation<PostUserModel>();
```

---

## 🛠️ Setup

### 1. Register the service

```csharp
builder.Services.AddValidify();
```

### 2. Create a FluentValidator

```csharp
public class PostUserModelValidator : AbstractValidator<PostUserModel>
{
    public PostUserModelValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(255).WithMessage("Must be less than 255 characters");

        // More rules...
    }
}
```

### 3. Register your FluentValidator's validators

```csharp
services
    .AddSingleton<IValidator<PostUserModel>, PostUserModelValidator>()
    .AddSingleton<IValidator<PutUserModel>, PutUserModelValidator>();
```

---

## 🔐 Native AOT Support

Validify is fully compatible with Native AOT for optimized, self-contained executables:

```xml
<IsAotCompatible>true</IsAotCompatible>
```

---

## 🧩 Register validation response types for source generation

To support HttpValidationProblemDetails responses in AOT scenarios, include it in your JsonSerializerContext:

```csharp
[JsonSerializable(typeof(HttpValidationProblemDetails))]
public partial class AppJsonSerializerContext : JsonSerializerContext;
```

This ensures that validation error responses can be properly serialized when using source-generated System.Text.Json.

---

## 📂 Project Structure

- `ValidifyRegistration.cs`: Registers all necessary services
- `ValidationEndpointExtensions.cs`: Adds `.WithValidation<T>()` to `RouteHandlerBuilder`
- `ValidationFilter.cs`: The filter that runs before your endpoint logic

---

## 📦 NuGet Package

📦 [nuget.org/packages/Kopticx.Validify](https://www.nuget.org/packages/Kopticx.Validify)

---

## 🧪 Why use Validify?

- Prevent invalid models from hitting your business logic
- Use FluentValidation's full feature set
- Centralize validation behavior with clean syntax
- Built for performance and developer experience

---

## 📝 License

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)

---

## 📄 Third-party licenses

Validify uses [FluentValidation](https://github.com/FluentValidation/FluentValidation), licensed under the [Apache 2.0 License](http://www.apache.org/licenses/LICENSE-2.0).

See `NOTICE` file for more information.
