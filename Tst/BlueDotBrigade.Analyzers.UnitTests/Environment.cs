namespace BlueDotBrigade.Analyzers
{
    using System;
    using System.IO;

    using BlueDotBrigade.DatenLokator.TestTools.Configuration;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Environment
    {
        private static bool _lokatorInitialized;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext _)
        {
            try
            {
                Lokator.Get().Setup();
                _lokatorInitialized = true;
            }
            catch (ArgumentException ex) when (ex.ParamName == "rootDirectoryPath")
            {
                // DatenLokator library (version 2.2.0) has a cross-platform path issue where
                // the root directory path is constructed with backslash instead of forward slash
                // on Linux/macOS (e.g., "\home\runner\..." instead of "/home/runner/...").
                // This causes Directory.Exists() to fail on non-Windows platforms.
                // Tests that rely on DatenLokator for test data files will be skipped,
                // but other tests (e.g., TerminologyValidatorTests, DslRuleParserTests) will still run.
                _lokatorInitialized = false;
                Console.WriteLine($"Warning: DatenLokator initialization failed (cross-platform path issue): {ex.Message}");
            }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            if (_lokatorInitialized)
            {
                Lokator.Get().TearDown();
            }
        }
    }
}
