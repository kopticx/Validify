using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Validify.SourceGenerator.Tests;

internal static class TestHelper
{
  private static readonly ImmutableArray<PortableExecutableReference> BaseReferences = ResolveBaseReferences();

  /// <summary>Runs the generator on <paramref name="source"/> and returns the driver for Verify.</summary>
  public static GeneratorDriver Run(string source)
  {
    var syntaxTree = CSharpSyntaxTree.ParseText(source);

    var compilation = CSharpCompilation.Create(
      assemblyName: "Validify.Tests.Sample",
      syntaxTrees: new[] { syntaxTree },
      references: BaseReferences,
      options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    GeneratorDriver driver = CSharpGeneratorDriver.Create(new ValidatorRegistrationGenerator());
    return driver.RunGenerators(compilation);
  }

  private static ImmutableArray<PortableExecutableReference> ResolveBaseReferences()
  {
    var trustedAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
      .Split(Path.PathSeparator);

    var builder = ImmutableArray.CreateBuilder<PortableExecutableReference>();
    foreach (var path in trustedAssemblies)
      builder.Add(MetadataReference.CreateFromFile(path));

    // Domain libraries the sample code references.
    builder.Add(MetadataReference.CreateFromFile(typeof(FluentValidation.AbstractValidator<>).Assembly.Location));
    builder.Add(MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection)
      .Assembly.Location));
    return builder.ToImmutable();
  }
}