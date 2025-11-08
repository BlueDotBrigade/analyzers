using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using BlueDotBrigade.Analyzers.Tests.Verifiers;
using BlueDotBrigade.DatenLokator.TestTools;
using BlueDotBrigade.DatenLokator.TestTools.Configuration;


namespace BlueDotBrigade.Analyzers.Tests
{
    [TestClass]
    public class DslTermAnalyzerTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext _)
        {
            Lokator.Get().Setup();
        }

        [TestMethod]
        public async Task NoDiagnostics_When_NoBlockedTerms()
        {
            var test = new CSharpAnalyzerVerifier.Test
            {
                TestCode = new Daten().AsString("code-clean.cs")
            };

            // DSL that doesn't block anything present in code
            var xml = new Daten().AsString("dsl-simple.xml");
            test.TestState.AdditionalFiles.Add(("dsl.config.xml", xml)); // filename the analyzer looks for

            await test.RunAsync();
        }

        [TestMethod]
        public async Task ReportsDiagnostics_For_Blocked_Terms_OneOff_And_Aliases()
        {
            var test = new CSharpAnalyzerVerifier.Test
            {
                TestCode = new Daten().AsString("code-violations.cs"),
            };

            var xml = new Daten().AsString("dsl-prefer-customer.xml"); // matches the new schema
            test.TestState.AdditionalFiles.Add(("dsl.config.xml", xml));

            // Expect diagnostic on the field 'ClientValue'
            test.ExpectedDiagnostics.Add(
                CSharpAnalyzerVerifier.Diagnostic("RC001").WithSpan(5, 21, 5, 32)); // ClientValue contains "Client"

            await test.RunAsync();
        }

        [TestMethod]
        public async Task Uses_SolutionLevel_File_When_ProjectLevel_Missing()
        {
            var test = new CSharpAnalyzerVerifier.Test
            {
                TestCode = new Daten().AsString("code-violations.cs"),
            };

            var xmlSolution = new Daten().AsString("dsl-prefer-customer.xml");
            test.TestState.AdditionalFiles.Add(("SolutionRoot/dsl.config.xml", xmlSolution));

            test.ExpectedDiagnostics.Add(
                CSharpAnalyzerVerifier.Diagnostic("RC001").WithSpan(5, 21, 5, 32)); // ClientValue

            await test.RunAsync();
        }

        [TestMethod]
        public async Task ProjectLevel_Overrides_SolutionLevel_When_MSBuildProjectDirectory_Provided()
        {
            var test = new CSharpAnalyzerVerifier.Test
            {
                TestCode = new Daten().AsString("code-violations.cs"),
            };

            // Project-level DSL that blocks nothing in the sample (only 'xyz')
            var xmlProject = new Daten().AsString("dsl-simple.xml");
            test.TestState.AdditionalFiles.Add(("src/TestProj/dsl.config.xml", xmlProject));

            // Solution-level DSL that would block 'Client'
            var xmlSolution = new Daten().AsString("dsl-prefer-customer.xml");
            test.TestState.AdditionalFiles.Add(("SolutionRoot/dsl.config.xml", xmlSolution));

            // Provide MSBuildProjectDirectory via .editorconfig as an AdditionalFile (test harness cannot add analyzer configs)
            test.TestState.AdditionalFiles.Add((
                "/.editorconfig",
                """
                root = true

                [*.cs]
                build_property.MSBuildProjectDirectory = src/TestProj
                """
            ));

            // Project-local DSL doesn't block 'Client', so no diagnostics expected
            await test.RunAsync();
        }

        [TestMethod]
        public async Task Falls_Back_To_DefaultDsl_When_File_Missing()
        {
            var sourceCode = new Daten().AsString("code-violations.cs");
            var test = new CSharpAnalyzerVerifier.Test
            {
                TestCode = sourceCode,
            };

            // Intentionally do NOT add any AdditionalFiles -> analyzer should synthesize default DSL
            // Default DSL blocks "Client" and "Cust" in favor of "Customer" (and is case sensitive),
            // plus the one-off also blocks "Client".

            test.TestCode = new Daten().AsString("code-violations-defaultdsl.cs");

            // Expected: the default DSL blocks "Client" (case-sensitive). Ensure identifier contains "Client".
            test.ExpectedDiagnostics.Add(
                CSharpAnalyzerVerifier.Diagnostic("RC001").WithSpan(5, 11, 5, 26)); // class ClientController

            await test.RunAsync();
        }
    }
}
