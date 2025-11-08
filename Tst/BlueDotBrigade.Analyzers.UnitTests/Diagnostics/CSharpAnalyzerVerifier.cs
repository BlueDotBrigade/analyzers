using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace BlueDotBrigade.Analyzers.Diagnostics
{
    internal static class CSharpAnalyzerVerifier
    {
        public class Test : CSharpAnalyzerTest<DslTerminologyAnalyzer, XUnitVerifier>
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
