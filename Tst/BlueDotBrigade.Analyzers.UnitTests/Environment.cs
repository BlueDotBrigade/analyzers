namespace BlueDotBrigade.Analyzers
{
    using BlueDotBrigade.DatenLokator.TestTools.Configuration;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Environment
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext _)
        {
            Lokator.Get().Setup();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Lokator.Get().TearDown();
        }
    }
}
