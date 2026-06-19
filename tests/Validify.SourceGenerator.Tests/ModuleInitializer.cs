using System.Runtime.CompilerServices;
using VerifyTests;

namespace Validify.SourceGenerator.Tests;

internal static class ModuleInitializer
{
  [ModuleInitializer]
  public static void Init()
  {
    DiffEngine.DiffRunner.Disabled = true;
    VerifySourceGenerators.Initialize();
  }
}