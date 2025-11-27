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
                // DatenLokator has a path separator issue on non-Windows platforms.
                // Tests that don't rely on DatenLokator will still run.
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
