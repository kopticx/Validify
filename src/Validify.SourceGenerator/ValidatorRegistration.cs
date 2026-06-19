namespace Validify.SourceGenerator;

/// <summary>One DI registration: a closed IValidator<> service mapped to its concrete validator.</summary>
internal readonly record struct ValidatorRegistration(string ServiceType, string ImplementationType);