// netstandard2.0 predates the IsExternalInit marker type the C# compiler needs to
// support `init`-only property accessors. This shim supplies it only for that TFM;
// net8.0 (and other modern TFMs) already provide it in the framework itself.
#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif
