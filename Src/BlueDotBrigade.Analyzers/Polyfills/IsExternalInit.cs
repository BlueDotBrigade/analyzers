// Polyfill to support C# 9/10 features (records/init) on .NET Standard 2.0
// Only compiled where the BCL doesn't provide it.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}