using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using BlueDotBrigade.DatenLokator.TestTools;

namespace BlueDotBrigade.Analyzers.Diagnostics
{
    [TestClass]
    public class DslTerminologyAnalyzerTests
    {
        [TestMethod]
        public async Task NoDiagnostics_When_NoBlockedTerms()
        {
            var test = new CSharpAnalyzerVerifier.Test
            {
                TestCode = new Daten().AsString("code-clean.cs")
            };

            var xml = new Daten().AsString("dsl-simple.xml");
            // Project-level DSL (simulated) that does not block anything in code
            test.TestState.AdditionalFiles.Add(("src/TestProj/dsl.config.xml", xml));
            test.TestState.AdditionalFiles.Add(("/.editorconfig", "build_property.MSBuildProjectDirectory = src/TestProj"));
            await test.RunAsync();
        }

        [TestMethod]
        public async Task ReportsDiagnostics_For_Blocked_Terms_ProjectLevel()
        {
            var test = new CSharpAnalyzerVerifier.Test
            {
                TestCode = new Daten().AsString("code-violations.cs"),
            };

            // Project-level DSL blocks Client/Cust
            var xmlProject = new Daten().AsString("dsl-prefer-customer.xml");
            test.TestState.AdditionalFiles.Add(("src/TestProj/dsl.config.xml", xmlProject));
            test.TestState.AdditionalFiles.Add(("/.editorconfig", "build_property.MSBuildProjectDirectory = src/TestProj"));

            test.ExpectedDiagnostics.Add(
                CSharpAnalyzerVerifier.Diagnostic("RC001").WithSpan(5,21,5,32)); // ClientValue contains Client

            await test.RunAsync();
        }

        [TestMethod]
        public async Task Uses_SolutionLevel_When_ProjectLevel_Missing()
        {
            var test = new CSharpAnalyzerVerifier.Test
            {
                TestCode = new Daten().AsString("code-violations.cs"),
            };

            // Only solution-level DSL provided
            var xmlSolution = new Daten().AsString("dsl-prefer-customer.xml");
            test.TestState.AdditionalFiles.Add(("SolutionRoot/dsl.config.xml", xmlSolution));

            test.ExpectedDiagnostics.Add(
                CSharpAnalyzerVerifier.Diagnostic("RC001").WithSpan(5,21,5,32));

            await test.RunAsync();
        }

        [TestMethod]
        public async Task ProjectLevel_Overrides_SolutionLevel()
        {
            var test = new CSharpAnalyzerVerifier.Test
            {
                TestCode = new Daten().AsString("code-violations.cs"),
            };

            // Project-level DSL that blocks nothing relevant
            var xmlProject = new Daten().AsString("dsl-simple.xml");
            test.TestState.AdditionalFiles.Add(("src/TestProj/dsl.config.xml", xmlProject));
            test.TestState.AdditionalFiles.Add(("/.editorconfig", "build_property.MSBuildProjectDirectory = src/TestProj"));

            // Solution-level DSL would block 'Client'
            var xmlSolution = new Daten().AsString("dsl-prefer-customer.xml");
            test.TestState.AdditionalFiles.Add(("SolutionRoot/dsl.config.xml", xmlSolution));

            // Expect no diagnostics because project-level overrides
            await test.RunAsync();
        }

        [TestMethod]
        public async Task Missing_Dsl_File_Reports_RC000_Only()
        {
            var test = new CSharpAnalyzerVerifier.Test
            {
                TestCode = new Daten().AsString("code-violations.cs"),
            };

            // No DSL files -> expect only configuration warning RC000 (no RC001)
            test.ExpectedDiagnostics.Add(
                CSharpAnalyzerVerifier.Diagnostic("RC000"));

            await test.RunAsync();
        }
    }
}
