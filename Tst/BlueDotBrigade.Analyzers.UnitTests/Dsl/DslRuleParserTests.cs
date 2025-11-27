namespace BlueDotBrigade.Analyzers.Dsl
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DslRuleParserTests
    {
        #region Valid XML Parsing

        [TestMethod]
        public void Parse_ReturnsRules_When_SingleTermWithBlock()
        {
            const string xml = """
                <dsl>
                    <term prefer="Customer" block="Client" case="sensitive"/>
                </dsl>
                """;

            var rules = DslRuleParser.Parse(xml);

            Assert.AreEqual(1, rules.Count);
            Assert.AreEqual("Client", rules[0].Blocked);
            Assert.AreEqual("Customer", rules[0].Preferred);
            Assert.IsTrue(rules[0].CaseSensitive);
        }

        [TestMethod]
        public void Parse_ReturnsRules_When_TermWithAliases()
        {
            const string xml = """
                <dsl>
                    <term prefer="Customer" case="sensitive">
                        <alias block="Client"/>
                        <alias block="Cust"/>
                    </term>
                </dsl>
                """;

            var rules = DslRuleParser.Parse(xml);

            Assert.AreEqual(2, rules.Count);
            Assert.AreEqual("Client", rules[0].Blocked);
            Assert.AreEqual("Customer", rules[0].Preferred);
            Assert.AreEqual("Cust", rules[1].Blocked);
            Assert.AreEqual("Customer", rules[1].Preferred);
        }

        [TestMethod]
        public void Parse_ReturnsRules_When_TermWithBlockAndAliases()
        {
            const string xml = """
                <dsl>
                    <term prefer="Customer" block="Client" case="sensitive">
                        <alias block="Cust"/>
                    </term>
                </dsl>
                """;

            var rules = DslRuleParser.Parse(xml);

            Assert.AreEqual(2, rules.Count);
            Assert.AreEqual("Client", rules[0].Blocked);
            Assert.AreEqual("Cust", rules[1].Blocked);
        }

        [TestMethod]
        public void Parse_ReturnsRules_When_CaseInsensitive()
        {
            const string xml = """
                <dsl>
                    <term prefer="Customer" block="Client" case="insensitive"/>
                </dsl>
                """;

            var rules = DslRuleParser.Parse(xml);

            Assert.AreEqual(1, rules.Count);
            Assert.IsFalse(rules[0].CaseSensitive);
        }

        [TestMethod]
        public void Parse_ReturnsRules_When_CaseAttributeMissing_DefaultsToSensitive()
        {
            const string xml = """
                <dsl>
                    <term prefer="Customer" block="Client"/>
                </dsl>
                """;

            var rules = DslRuleParser.Parse(xml);

            Assert.AreEqual(1, rules.Count);
            Assert.IsTrue(rules[0].CaseSensitive);
        }

        [TestMethod]
        public void Parse_ReturnsMultipleRules_When_MultipleTerms()
        {
            const string xml = """
                <dsl>
                    <term prefer="Customer" block="Client" case="sensitive"/>
                    <term prefer="FileName" block="Filename" case="sensitive"/>
                    <term prefer="MetaData" block="Metadata" case="insensitive"/>
                </dsl>
                """;

            var rules = DslRuleParser.Parse(xml);

            Assert.AreEqual(3, rules.Count);

            var customerRule = rules.First(r => r.Preferred == "Customer");
            Assert.AreEqual("Client", customerRule.Blocked);
            Assert.IsTrue(customerRule.CaseSensitive);

            var fileNameRule = rules.First(r => r.Preferred == "FileName");
            Assert.AreEqual("Filename", fileNameRule.Blocked);
            Assert.IsTrue(fileNameRule.CaseSensitive);

            var metaDataRule = rules.First(r => r.Preferred == "MetaData");
            Assert.AreEqual("Metadata", metaDataRule.Blocked);
            Assert.IsFalse(metaDataRule.CaseSensitive);
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Parse_ReturnsEmptyList_When_EmptyDsl()
        {
            const string xml = "<dsl></dsl>";

            var rules = DslRuleParser.Parse(xml);

            Assert.AreEqual(0, rules.Count);
        }

        [TestMethod]
        public void Parse_ReturnsEmptyList_When_RootIsNotDsl()
        {
            const string xml = "<config><term prefer=\"Customer\" block=\"Client\"/></config>";

            var rules = DslRuleParser.Parse(xml);

            Assert.AreEqual(0, rules.Count);
        }

        [TestMethod]
        public void Parse_SkipsTerm_When_PreferAttributeMissing()
        {
            const string xml = """
                <dsl>
                    <term block="Client" case="sensitive"/>
                </dsl>
                """;

            var rules = DslRuleParser.Parse(xml);

            Assert.AreEqual(0, rules.Count);
        }

        [TestMethod]
        public void Parse_SkipsTerm_When_BlockAndAliasesAreMissing()
        {
            const string xml = """
                <dsl>
                    <term prefer="Customer" case="sensitive"/>
                </dsl>
                """;

            var rules = DslRuleParser.Parse(xml);

            Assert.AreEqual(0, rules.Count);
        }

        [TestMethod]
        public void Parse_SkipsAlias_When_BlockAttributeMissing()
        {
            const string xml = """
                <dsl>
                    <term prefer="Customer" case="sensitive">
                        <alias/>
                        <alias block="Cust"/>
                    </term>
                </dsl>
                """;

            var rules = DslRuleParser.Parse(xml);

            Assert.AreEqual(1, rules.Count);
            Assert.AreEqual("Cust", rules[0].Blocked);
        }

        [TestMethod]
        public void Parse_ThrowsXmlException_When_InvalidXml()
        {
            const string xml = "<dsl><term prefer=\"Customer\"";

            try
            {
                DslRuleParser.Parse(xml);
                Assert.Fail("Expected XmlException was not thrown");
            }
            catch (System.Xml.XmlException)
            {
                // Expected behavior
            }
        }

        #endregion

        #region Real-World Scenarios

        [TestMethod]
        public void Parse_HandlesDefaultDslXml()
        {
            var rules = DslRuleParser.Parse(DslDefaults.DefaultDslXml);

            // The default DSL has 2 terms (one with 1 block, one with 2 aliases)
            Assert.IsTrue(rules.Count >= 3);
        }

        [TestMethod]
        public void Parse_ReturnsRules_For_MetadataScenario()
        {
            const string xml = """
                <dsl>
                    <term prefer="MetaData" block="Metadata" case="sensitive"/>
                </dsl>
                """;

            var rules = DslRuleParser.Parse(xml);

            Assert.AreEqual(1, rules.Count);
            Assert.AreEqual("Metadata", rules[0].Blocked);
            Assert.AreEqual("MetaData", rules[0].Preferred);
        }

        [TestMethod]
        public void Parse_ReturnsRules_For_FileNameScenario()
        {
            const string xml = """
                <dsl>
                    <term prefer="FileName" block="Filename" case="sensitive"/>
                </dsl>
                """;

            var rules = DslRuleParser.Parse(xml);

            Assert.AreEqual(1, rules.Count);
            Assert.AreEqual("Filename", rules[0].Blocked);
            Assert.AreEqual("FileName", rules[0].Preferred);
        }

        #endregion
    }
}
