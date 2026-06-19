namespace Validify.SourceGenerator;

/// <summary>One DI registration: a closed <c>IValidator&lt;T&gt;</c> service mapped to its concrete validator.</summary>
internal readonly record struct ValidatorRegistration(string ServiceType, string ImplementationType);