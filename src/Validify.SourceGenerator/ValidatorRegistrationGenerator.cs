using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Validify.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class ValidatorRegistrationGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat FullyQualified = SymbolDisplayFormat.FullyQualifiedFormat;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var local = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax { BaseList: not null },
                transform: static (ctx, _) => GetLocalValidators(ctx))
            .Where(static a => a.Count > 0)
            .Collect();

        var referenced = context.CompilationProvider
            .Select(static (compilation, _) => GetReferencedValidators(compilation));

        var combined = local.Combine(referenced);

        context.RegisterSourceOutput(combined, static (spc, pair) =>
        {
            var all = Flatten(pair.Left).AsImmutableArray().AddRange(pair.Right.AsImmutableArray());
            Emit(spc, new EquatableArray<ValidatorRegistration>(all));
        });
    }

    private static EquatableArray<ValidatorRegistration> GetReferencedValidators(Compilation compilation)
    {
        var builder = ImmutableArray.CreateBuilder<ValidatorRegistration>();

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly)
                continue;

            if (!ReferencesFluentValidation(assembly))
                continue;

            foreach (var type in GetAllTypes(assembly.GlobalNamespace))
            {
                if (!IsConcreteCandidate(type) || type.DeclaredAccessibility != Accessibility.Public)
                    continue;

                foreach (var serviceType in GetValidatorServiceTypes(type))
                    builder.Add(new ValidatorRegistration(serviceType, type.ToDisplayString(FullyQualified)));
            }
        }

        return new EquatableArray<ValidatorRegistration>(builder.ToImmutable());
    }

    private static bool ReferencesFluentValidation(IAssemblySymbol assembly) =>
        assembly.Modules.Any(module =>
            module.ReferencedAssemblies.Any(id => id.Name == "FluentValidation"));

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol root)
    {
        var stack = new Stack<INamespaceOrTypeSymbol>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            foreach (var member in current.GetMembers())
            {
                switch (member)
                {
                    case INamespaceSymbol ns:
                        stack.Push(ns);
                        break;
                    case INamedTypeSymbol type:
                        yield return type;
                        foreach (var nested in type.GetTypeMembers())
                            stack.Push(nested);
                        break;
                }
            }
        }
    }

    private static EquatableArray<ValidatorRegistration> GetLocalValidators(GeneratorSyntaxContext ctx)
    {
        if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) is not INamedTypeSymbol symbol)
            return EquatableArray<ValidatorRegistration>.Empty;

        if (!IsConcreteCandidate(symbol))
            return EquatableArray<ValidatorRegistration>.Empty;

        var builder = ImmutableArray.CreateBuilder<ValidatorRegistration>();
        foreach (var serviceType in GetValidatorServiceTypes(symbol))
            builder.Add(new ValidatorRegistration(serviceType, symbol.ToDisplayString(FullyQualified)));

        return new EquatableArray<ValidatorRegistration>(builder.ToImmutable());
    }

    /// <summary>True when the type can be instantiated and registered as a validator.</summary>
    private static bool IsConcreteCandidate(INamedTypeSymbol symbol) =>
        symbol is { TypeKind: TypeKind.Class, IsAbstract: false, IsStatic: false, IsGenericType: false };

    /// <summary>Fully-qualified <c>IValidator&lt;T&gt;</c> strings this type implements.</summary>
    private static IEnumerable<string> GetValidatorServiceTypes(INamedTypeSymbol symbol)
    {
        foreach (var iface in symbol.AllInterfaces)
        {
            if (iface is { MetadataName: "IValidator`1", TypeArguments.Length: 1 } &&
                iface.ContainingNamespace.ToDisplayString() == "FluentValidation")
            {
                yield return iface.ToDisplayString(FullyQualified);
            }
        }
    }

    private static EquatableArray<ValidatorRegistration> Flatten(ImmutableArray<EquatableArray<ValidatorRegistration>> groups)
    {
        var builder = ImmutableArray.CreateBuilder<ValidatorRegistration>();
        foreach (var group in groups)
            builder.AddRange(group.AsImmutableArray());
        return new EquatableArray<ValidatorRegistration>(builder.ToImmutable());
    }

    private static void Emit(SourceProductionContext context, EquatableArray<ValidatorRegistration> registrations)
    {
        var ordered = registrations.AsImmutableArray()
            .Distinct()
            .OrderBy(static r => r.ServiceType, System.StringComparer.Ordinal)
            .ThenBy(static r => r.ImplementationType, System.StringComparer.Ordinal)
            .ToImmutableArray();

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("namespace Microsoft.Extensions.DependencyInjection");
        sb.AppendLine("{");
        sb.AppendLine("    using global::Microsoft.Extensions.DependencyInjection.Extensions;");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>Auto-generated FluentValidation validator registrations.</summary>");
        sb.AppendLine("    public static class ValidifyGeneratedRegistration");
        sb.AppendLine("    {");
        sb.AppendLine("        /// <summary>Registers every discovered FluentValidation validator.</summary>");
        sb.AppendLine("        public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddValidifyValidators(");
        sb.AppendLine("            this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services,");
        sb.AppendLine("            global::Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime = global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped)");
        sb.AppendLine("        {");
        foreach (var r in ordered)
        {
            sb.AppendLine(
                $"            services.TryAdd(new global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof({r.ServiceType}), typeof({r.ImplementationType}), lifetime));");
        }
        sb.AppendLine("            return services;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource("ValidifyGeneratedRegistration.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
