using Microsoft.CodeAnalysis;

namespace Validify.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class ValidatorRegistrationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Implemented in later tasks.
    }
}