namespace BlueDotBrigade.Analyzers.Dsl
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TerminologyValidatorTests
    {
        #region Valid Identifiers (Pass)

        [TestMethod]
        public void Validate_ReturnsValid_When_IdentifierIsEmpty()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate(string.Empty);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ReturnsValid_When_NoRulesConfigured()
        {
            var rules = new List<TerminologyRule>();
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("AnyCust");

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ReturnsValid_When_IdentifierIsExactlyPreferredTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("Customer");

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ReturnsValid_When_IdentifierContainsPreferredTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("CustomerCount");

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ReturnsValid_When_IdentifierEndsWithPreferredTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("PreferredCustomer");

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ReturnsValid_When_BlockedTermNotInIdentifier()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("OrderCount");

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ReturnsValid_When_CaseSensitive_And_DifferentCase()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("custValue");

            Assert.IsTrue(result.IsValid);
        }

        #endregion

        #region Invalid Identifiers (Fail)

        [TestMethod]
        public void Validate_ReturnsViolation_When_IdentifierStartsWithBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("CustValue");

            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.ViolatedRule);
            Assert.AreEqual("Cust", result.ViolatedRule.Blocked);
        }

        [TestMethod]
        public void Validate_ReturnsViolation_When_IdentifierEndsWithBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("PreferredCust");

            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.ViolatedRule);
            Assert.AreEqual("Cust", result.ViolatedRule.Blocked);
        }

        [TestMethod]
        public void Validate_ReturnsViolation_When_IdentifierContainsBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("TheCustValue");

            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.ViolatedRule);
            Assert.AreEqual("Cust", result.ViolatedRule.Blocked);
        }

        [TestMethod]
        public void Validate_ReturnsViolation_When_IdentifierIsExactlyBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("Cust");

            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.ViolatedRule);
            Assert.AreEqual("Cust", result.ViolatedRule.Blocked);
        }

        [TestMethod]
        public void Validate_ReturnsViolation_When_CaseInsensitive_And_DifferentCase()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", false)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("custValue");

            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.ViolatedRule);
            Assert.AreEqual("Cust", result.ViolatedRule.Blocked);
        }

        #endregion

        #region Edge Cases with Metadata/MetaData style conflicts

        [TestMethod]
        public void Validate_ReturnsValid_When_IdentifierContainsPreferredTerm_MetaData()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Metadata", "MetaData", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("MetaDataValue");

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ReturnsViolation_When_IdentifierContainsBlockedTerm_Metadata()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Metadata", "MetaData", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("MetadataValue");

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Metadata", result.ViolatedRule.Blocked);
        }

        [TestMethod]
        public void Validate_ReturnsValid_When_IdentifierContainsPreferredTerm_FileName()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Filename", "FileName", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("FileName");

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ReturnsViolation_When_IdentifierContainsBlockedTerm_Filename()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Filename", "FileName", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("Filename");

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Filename", result.ViolatedRule.Blocked);
        }

        #endregion

        #region Multiple Rules

        [TestMethod]
        public void Validate_ReturnsViolation_When_FirstRuleMatches()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true),
                new TerminologyRule("Client", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("CustValue");

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Cust", result.ViolatedRule.Blocked);
        }

        [TestMethod]
        public void Validate_ReturnsViolation_When_SecondRuleMatches()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true),
                new TerminologyRule("Client", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("ClientValue");

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Client", result.ViolatedRule.Blocked);
        }

        [TestMethod]
        public void Validate_ReturnsValid_When_NoRuleMatches()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true),
                new TerminologyRule("Client", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.Validate("CustomerValue");

            Assert.IsTrue(result.IsValid);
        }

        #endregion
    }
}
