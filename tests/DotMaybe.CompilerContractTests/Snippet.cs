using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotMaybe.CompilerContractTests;

/// <summary>
/// Compiles C# source against DotMaybe exactly as a consumer project would: C# 15, nullable enabled,
/// with the SDK's own compiler.
/// </summary>
internal static class Snippet
{
    private static readonly Lazy<ImmutableArray<MetadataReference>> References = new(LoadReferences);

    private static readonly CSharpParseOptions ParseOptions = new(CSharp15());

    private static readonly CSharpCompilationOptions CompilationOptions =
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithNullableContextOptions(NullableContextOptions.Enable)
            .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
            {
                // Assembly-unification noise that the SDK also suppresses by default.
                ["CS1701"] = ReportDiagnostic.Suppress,
                ["CS1702"] = ReportDiagnostic.Suppress,
            });

    /// <summary>
    /// Compiles <paramref name="source"/> and returns every warning and error the compiler reports.
    /// </summary>
    public static CompilationResult Compile(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, ParseOptions);
        var compilation = CSharpCompilation.Create(
            assemblyName: "DotMaybeConsumer",
            syntaxTrees: [tree],
            references: References.Value,
            options: CompilationOptions);

        var diagnostics = compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning)
            .ToImmutableArray();

        return new CompilationResult(source, diagnostics);
    }

    private static LanguageVersion CSharp15() =>
        LanguageVersionFacts.TryParse("15", out var version)
            ? version
            : throw new InvalidOperationException("The SDK's compiler does not know C# 15. Check global.json.");

    // The running test host already loads the .NET 11 framework and DotMaybe, so its trusted platform
    // assemblies are exactly what a consumer compiles against.
    private static ImmutableArray<MetadataReference> LoadReferences()
    {
        var trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string
            ?? throw new InvalidOperationException("The test host does not expose TRUSTED_PLATFORM_ASSEMBLIES.");

        var framework = trustedAssemblies
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(IsFrameworkAssembly);

        return framework
            .Append(typeof(Maybe<>).Assembly.Location)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToImmutableArray();
    }

    private static bool IsFrameworkAssembly(string path)
    {
        var name = Path.GetFileName(path);
        return name.StartsWith("System.", StringComparison.Ordinal)
            || name is "mscorlib.dll" or "netstandard.dll";
    }
}
