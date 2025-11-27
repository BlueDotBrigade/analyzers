namespace BlueDotBrigade.Analyzers.Dsl
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TerminologyValidatorTests
    {
        #region Valid Identifiers (Pass)

        [TestMethod]
        public void GetViolation_ReturnsNull_When_IdentifierIsEmpty()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation(string.Empty);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_NoRulesConfigured()
        {
            var rules = new List<TerminologyRule>();
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("AnyCust");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_IdentifierIsExactlyPreferredTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("Customer");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_IdentifierContainsPreferredTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("CustomerCount");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_IdentifierEndsWithPreferredTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("PreferredCustomer");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_BlockedTermNotInIdentifier()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("OrderCount");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_CaseSensitive_And_DifferentCase()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("custValue");

            Assert.IsNull(result);
        }

        #endregion

        #region Invalid Identifiers (Fail)

        [TestMethod]
        public void GetViolation_ReturnsRule_When_IdentifierStartsWithBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("CustValue");

            Assert.IsNotNull(result);
            Assert.AreEqual("Cust", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_IdentifierEndsWithBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("PreferredCust");

            Assert.IsNotNull(result);
            Assert.AreEqual("Cust", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_IdentifierContainsBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("TheCustValue");

            Assert.IsNotNull(result);
            Assert.AreEqual("Cust", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_IdentifierIsExactlyBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("Cust");

            Assert.IsNotNull(result);
            Assert.AreEqual("Cust", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_CaseInsensitive_And_DifferentCase()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", false)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("custValue");

            Assert.IsNotNull(result);
            Assert.AreEqual("Cust", result.Blocked);
        }

        #endregion

        #region Edge Cases with Metadata/MetaData style conflicts

        [TestMethod]
        public void GetViolation_ReturnsNull_When_IdentifierContainsPreferredTerm_MetaData()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Metadata", "MetaData", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("MetaDataValue");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_IdentifierContainsBlockedTerm_Metadata()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Metadata", "MetaData", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("MetadataValue");

            Assert.IsNotNull(result);
            Assert.AreEqual("Metadata", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_IdentifierContainsPreferredTerm_FileName()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Filename", "FileName", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("FileName");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_IdentifierContainsBlockedTerm_Filename()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Filename", "FileName", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("Filename");

            Assert.IsNotNull(result);
            Assert.AreEqual("Filename", result.Blocked);
        }

        #endregion

        #region Multiple Rules

        [TestMethod]
        public void GetViolation_ReturnsRule_When_FirstRuleMatches()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true),
                new TerminologyRule("Client", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("CustValue");

            Assert.IsNotNull(result);
            Assert.AreEqual("Cust", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_SecondRuleMatches()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true),
                new TerminologyRule("Client", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("ClientValue");

            Assert.IsNotNull(result);
            Assert.AreEqual("Client", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_NoRuleMatches()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true),
                new TerminologyRule("Client", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("CustomerValue");

            Assert.IsNull(result);
        }

        #endregion
    }
}
