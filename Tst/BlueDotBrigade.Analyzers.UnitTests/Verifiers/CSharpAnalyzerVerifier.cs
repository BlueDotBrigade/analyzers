using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using BlueDotBrigade.Analyzers;

namespace BlueDotBrigade.Analyzers.Tests.Verifiers
{
    internal static class CSharpAnalyzerVerifier
    {
        public class Test : CSharpAnalyzerTest<DslTermAnalyzer, XUnitVerifier>
        {
            public Test()
            {
                SolutionTransforms.Add((solution, projectId) =>
                {
                    var project = solution.GetProject(projectId)!;
                    solution = solution.WithProjectParseOptions(
                        projectId,
                        ((Microsoft.CodeAnalysis.CSharp.CSharpParseOptions)project.ParseOptions!)
                        .WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview));
                    return solution;
                });
            }
        }

        public static DiagnosticResult Diagnostic(string id)
            => new DiagnosticResult(id, DiagnosticSeverity.Warning);
    }
}
