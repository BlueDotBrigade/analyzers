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

        #region Acronyms (IO vs Io, HTTP vs Http)

        [TestMethod]
        public void GetViolation_ReturnsNull_When_IdentifierUsesPreferredAcronym_IO()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Io", "IO", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("IOHandler");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_IdentifierUsesBlockedAcronym_Io()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Io", "IO", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("IoHandler");

            Assert.IsNotNull(result);
            Assert.AreEqual("Io", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_IdentifierUsesPreferredAcronym_HTTP()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Http", "HTTP", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("HTTPClient");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_IdentifierUsesBlockedAcronym_Http()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Http", "HTTP", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("HttpClient");

            Assert.IsNotNull(result);
            Assert.AreEqual("Http", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_CaseInsensitiveAcronym_And_DifferentCase()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Http", "HTTP", false)
            };
            var validator = new TerminologyValidator(rules);

            // Case-insensitive: "HTTP" contains "http" as a match for "Http"
            // But since "HTTP" is exactly the preferred term, it should not report
            var result = validator.GetViolation("HTTP");

            Assert.IsNull(result);
        }

        #endregion

        #region Compound Words (FileName vs Filename, MetaData vs Metadata variations)

        [TestMethod]
        public void GetViolation_ReturnsNull_When_IdentifierUsesPreferredCompound_MetaData()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Metadata", "MetaData", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("GetMetaData");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_IdentifierUsesBlockedCompound_Metadata()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Metadata", "MetaData", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("GetMetadata");

            Assert.IsNotNull(result);
            Assert.AreEqual("Metadata", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_LowercaseMetadata_Not_Blocked_CaseSensitive()
        {
            // When blocking "Metadata" (capital M) with case-sensitive rule,
            // "metadata" (lowercase m) should NOT be blocked
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Metadata", "MetaData", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("metadata");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_LowercaseMetadata_And_CaseInsensitiveRule_BecausePreferredMatches()
        {
            // When using case-insensitive rule with blocked="Metadata" and prefer="MetaData",
            // "metadata" case-insensitively matches both the blocked and preferred terms.
            // The validator sees that the preferred term "MetaData" aligns at position 0
            // and treats it as allowed (not a violation).
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Metadata", "MetaData", false)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("metadata");

            // Current behavior: returns null because case-insensitively "metadata" matches "MetaData"
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_LowercaseIdentifier_ContainsBlockedTerm_CaseInsensitive()
        {
            // Case-insensitive rule where blocked term is clearly different from preferred
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", false)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("myCustValue");

            Assert.IsNotNull(result);
            Assert.AreEqual("Cust", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_metaData_Style_Not_Blocked()
        {
            // "metaData" is a valid camelCase style that differs from both "Metadata" and "MetaData"
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Metadata", "MetaData", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("metaData");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_IdentifierUsesPreferredCompound_FileName()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Filename", "FileName", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("GetFileName");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_IdentifierUsesBlockedCompound_Filename()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Filename", "FileName", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("GetFilename");

            Assert.IsNotNull(result);
            Assert.AreEqual("Filename", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_fileName_Style_Not_Blocked()
        {
            // "fileName" is camelCase which differs from "Filename"
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Filename", "FileName", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("fileName");

            Assert.IsNull(result);
        }

        #endregion

        #region Underscore-Prefixed Fields

        [TestMethod]
        public void GetViolation_ReturnsRule_When_UnderscorePrefixedField_ContainsBlockedTerm()
        {
            // Note: The analyzer currently checks the entire identifier name.
            // Underscore-prefixed fields like "_custValue" still contain "Cust" and will be flagged.
            // This test validates current behavior - if ignoring underscore prefixes is required,
            // the TerminologyValidator would need to be enhanced.
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("_CustValue");

            Assert.IsNotNull(result);
            Assert.AreEqual("Cust", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_UnderscorePrefixedField_ContainsPreferredTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("_CustomerValue");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_UnderscorePrefixedField_NoBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("_orderCount");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_DoubleUnderscorePrefix_NoBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("__orderValue");

            Assert.IsNull(result);
        }

        #endregion

        #region Underscore/Hyphen Between Words (snake_case, kebab-style)

        [TestMethod]
        public void GetViolation_ReturnsRule_When_SnakeCase_ContainsBlockedTerm()
        {
            // snake_case identifiers like "cust_value" still contain "cust"
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("cust", "customer", false)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("cust_value");

            Assert.IsNotNull(result);
            Assert.AreEqual("cust", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_SnakeCase_ContainsPreferredTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("cust", "customer", false)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("customer_value");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_SnakeCase_ContainsBlockedTerm_CaseSensitive()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("Cust_Value");

            Assert.IsNotNull(result);
            Assert.AreEqual("Cust", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_SnakeCase_NoBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("order_value");

            Assert.IsNull(result);
        }

        #endregion

        #region Case-Based Naming (Variables, Constants)

        [TestMethod]
        public void GetViolation_ReturnsNull_When_LowercaseVariable_UsesPreferredTerm()
        {
            // Variables typically start with lowercase in C#
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("cust", "customer", false)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("customerValue");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_LowercaseVariable_ContainsBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("cust", "customer", false)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("custValue");

            Assert.IsNotNull(result);
            Assert.AreEqual("cust", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_UppercaseConstant_UsesPreferredTerm()
        {
            // Constants typically use UPPER_CASE or PascalCase
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("CUST", "CUSTOMER", false)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("CUSTOMER_ID");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_UppercaseConstant_ContainsBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("CUST", "CUSTOMER", false)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("CUST_ID");

            Assert.IsNotNull(result);
            Assert.AreEqual("CUST", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_PascalCaseConstant_UsesPreferredTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("CustomerMaxCount");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_PascalCaseConstant_ContainsBlockedTerm()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("CustMaxCount");

            Assert.IsNotNull(result);
            Assert.AreEqual("Cust", result.Blocked);
        }

        #endregion

        #region Edge Cases - Partial Matches and Boundaries

        [TestMethod]
        public void GetViolation_ReturnsNull_When_BlockedTermIsSubstringOfDifferentWord()
        {
            // "Custom" contains "Cust" but is a different word, should not be blocked
            // when the preferred term "Customer" is also present in matching position
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("CustomerId");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_MultipleBlockedOccurrences()
        {
            // Identifier contains blocked term multiple times
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("CustToCustMapping");

            Assert.IsNotNull(result);
            Assert.AreEqual("Cust", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_PreferredTermOccursMultipleTimes()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("CustomerToCustomerMapping");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_EmptyBlockedTerm()
        {
            // Edge case: Empty blocked term matches at every position (index 0, etc.)
            // The current implementation returns a rule because IndexOf("", 0) returns 0,
            // and an empty preferred term at index 0 + 0 doesn't match.
            // This is documented behavior - DSL configurations should avoid empty blocked terms.
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("AnyIdentifier");

            // Current behavior: returns the rule because empty string matches at position 0
            Assert.IsNotNull(result);
            Assert.AreEqual("", result.Blocked);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_IdentifierIsNull()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("Cust", "Customer", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation(null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_RulesListIsNull()
        {
            var validator = new TerminologyValidator(null);

            var result = validator.GetViolation("CustValue");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsNull_When_SingleCharacterBlockedTerm_NotFound()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("X", "Extended", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("YValue");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetViolation_ReturnsRule_When_SingleCharacterBlockedTerm_Found()
        {
            var rules = new List<TerminologyRule>
            {
                new TerminologyRule("X", "Extended", true)
            };
            var validator = new TerminologyValidator(rules);

            var result = validator.GetViolation("XValue");

            Assert.IsNotNull(result);
            Assert.AreEqual("X", result.Blocked);
        }

        #endregion
    }
}
