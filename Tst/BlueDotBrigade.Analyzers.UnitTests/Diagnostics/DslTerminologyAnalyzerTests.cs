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
                CSharpAnalyzerVerifier.Diagnostic("BDB001").WithSpan(5,21,5,32)); // ClientValue contains Client

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
                CSharpAnalyzerVerifier.Diagnostic("BDB001").WithSpan(5,21,5,32));

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
        public async Task Missing_Dsl_File_Reports_BDB000_Only()
        {
            var test = new CSharpAnalyzerVerifier.Test
            {
                TestCode = new Daten().AsString("code-violations.cs"),
            };

            // No DSL files -> expect only configuration warning BDB000 (no BDB001)
            test.ExpectedDiagnostics.Add(
                CSharpAnalyzerVerifier.Diagnostic("BDB000"));

            await test.RunAsync();
        }

        [TestMethod]
        public async Task ClassName_Pass_When_UsingPreferredTerm()
        {
            var test = CreateTest("""
namespace Sample;

public class Customer
{
}
""");

            await test.RunAsync();
        }

        [TestMethod]
        public async Task FieldName_Pass_When_UsingPreferredTerm()
        {
            var test = CreateTest("""
namespace Sample;

public class Example
{
    private int CustomerCount;
}
""");

            await test.RunAsync();
        }

        [TestMethod]
        public async Task PropertyName_Pass_When_UsingPreferredTerm()
        {
            var test = CreateTest("""
namespace Sample;

public class Example
{
    public string PreferredCustomer { get; set; }
}
""");

            await test.RunAsync();
        }

        [TestMethod]
        public async Task MethodName_Pass_When_UsingPreferredTerm()
        {
            var test = CreateTest("""
namespace Sample;

public class Example
{
    public void CustomerInfluencer() { }
}
""");

            await test.RunAsync();
        }

        [TestMethod]
        public async Task ParameterName_Pass_When_UsingPreferredTerm()
        {
            var test = CreateTest("""
namespace Sample;

public class Example
{
    public void Process(string PreferredCustomer) { }
}
""");

            await test.RunAsync();
        }

        [TestMethod]
        public async Task LocalVariableName_Pass_When_UsingPreferredTerm()
        {
            var test = CreateTest("""
namespace Sample;

public class Example
{
    public void Process()
    {
        var customerValue = 0;
    }
}
""");

            await test.RunAsync();
        }

        [TestMethod]
        public async Task ClassName_Fail_When_UsingBlockedTerm()
        {
            var test = CreateTest("""
namespace Sample;

public class Cust
{
}
""");

            test.ExpectedDiagnostics.Add(
                CSharpAnalyzerVerifier.Diagnostic("BDB001").WithSpan(3,14,3,18));

            await test.RunAsync();
        }

        [TestMethod]
        public async Task FieldName_Fail_When_UsingBlockedTerm()
        {
            var test = CreateTest("""
namespace Sample;

public class Example
{
    private int CustCount;
}
""");

            test.ExpectedDiagnostics.Add(
                CSharpAnalyzerVerifier.Diagnostic("BDB001").WithSpan(5,17,5,26));

            await test.RunAsync();
        }

        [TestMethod]
        public async Task PropertyName_Fail_When_UsingBlockedTerm()
        {
            var test = CreateTest("""
namespace Sample;

public class Example
{
    public string PreferredCust { get; set; }
}
""");

            test.ExpectedDiagnostics.Add(
                CSharpAnalyzerVerifier.Diagnostic("BDB001").WithSpan(5,19,5,32));

            await test.RunAsync();
        }

        [TestMethod]
        public async Task MethodName_Fail_When_UsingBlockedTerm()
        {
            var test = CreateTest("""
namespace Sample;

public class Example
{
    public void CustInfluencer() { }
}
""");

            test.ExpectedDiagnostics.Add(
                CSharpAnalyzerVerifier.Diagnostic("BDB001").WithSpan(5,17,5,31));

            await test.RunAsync();
        }

        [TestMethod]
        public async Task ParameterName_Fail_When_UsingBlockedTerm()
        {
            var test = CreateTest("""
namespace Sample;

public class Example
{
    public void Process(string PreferredCust) { }
}
""");

            test.ExpectedDiagnostics.Add(
                CSharpAnalyzerVerifier.Diagnostic("BDB001").WithSpan(5,32,5,45));

            await test.RunAsync();
        }

        [TestMethod]
        public async Task LocalVariableName_Fail_When_UsingBlockedTerm()
        {
            var test = CreateTest("""
namespace Sample;

public class Example
{
    public void Process()
    {
        var CustValue = 0;
    }
}
""");

            test.ExpectedDiagnostics.Add(
                CSharpAnalyzerVerifier.Diagnostic("BDB001").WithSpan(7,13,7,22));

            await test.RunAsync();
        }

        private static CSharpAnalyzerVerifier.Test CreateTest(string code)
        {
            var test = new CSharpAnalyzerVerifier.Test
            {
                TestCode = code,
            };

            var xml = new Daten().AsString("dsl-prefer-customer-block-cust.xml");

            test.TestState.AdditionalFiles.Add(("src/TestProj/dsl.config.xml", xml));
            test.TestState.AdditionalFiles.Add(("/.editorconfig", "build_property.MSBuildProjectDirectory = src/TestProj"));

            return test;
        }
    }
}
