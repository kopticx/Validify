// Polyfill for the C# `init` accessor / positional record structs on netstandard2.0,
// which does not ship System.Runtime.CompilerServices.IsExternalInit.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}