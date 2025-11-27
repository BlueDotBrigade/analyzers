namespace BlueDotBrigade.Analyzers.Dsl;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents the result of validating an identifier against terminology rules.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the identifier is valid (no blocked terms found).
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the rule that was violated, or <c>null</c> if the identifier is valid.
    /// </summary>
    public TerminologyRule? ViolatedRule { get; }

    private ValidationResult(bool isValid, TerminologyRule? violatedRule)
    {
        IsValid = isValid;
        ViolatedRule = violatedRule;
    }

    /// <summary>
    /// Creates a successful validation result indicating the identifier is valid.
    /// </summary>
    public static ValidationResult Success { get; } = new(true, null);

    /// <summary>
    /// Creates a failed validation result indicating a rule was violated.
    /// </summary>
    /// <param name="violatedRule">The rule that was violated.</param>
    /// <returns>A validation result representing the violation.</returns>
    public static ValidationResult Violation(TerminologyRule violatedRule) => new(false, violatedRule);
}

/// <summary>
/// Validates identifiers against terminology rules to ensure blocked terms are not used.
/// </summary>
/// <remarks>
/// This validator checks if an identifier contains any blocked terms and reports violations.
/// It supports both case-sensitive and case-insensitive matching, and handles edge cases
/// where the preferred term contains the blocked term (e.g., "Customer" containing "Cust").
/// </remarks>
public sealed class TerminologyValidator
{
    private readonly List<TerminologyRule> _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="TerminologyValidator"/> class.
    /// </summary>
    /// <param name="rules">The list of terminology rules to validate against.</param>
    public TerminologyValidator(List<TerminologyRule> rules)
    {
        _rules = rules ?? new List<TerminologyRule>();
    }

    /// <summary>
    /// Validates an identifier against all configured terminology rules.
    /// </summary>
    /// <param name="identifierName">The identifier name to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating whether the identifier is valid.</returns>
    public ValidationResult Validate(string identifierName)
    {
        if (string.IsNullOrEmpty(identifierName) || _rules.Count == 0)
        {
            return ValidationResult.Success;
        }

        foreach (var rule in _rules)
        {
            var comparison = rule.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            // Fast-path: if the identifier is exactly the preferred term, do not report
            if (!string.IsNullOrEmpty(rule.Preferred) && string.Equals(identifierName, rule.Preferred, comparison))
            {
                continue;
            }

            var searchStart = 0;
            while (true)
            {
                var idx = identifierName.IndexOf(rule.Blocked, searchStart, comparison);
                if (idx < 0)
                {
                    break;
                }

                // If this blocked occurrence aligns with the preferred term at the same position,
                // treat it as allowed (e.g., "Customer" contains "Cust" at index 0 but is preferred)
                if (!string.IsNullOrEmpty(rule.Preferred)
                    && idx + rule.Preferred.Length <= identifierName.Length
                    && identifierName.IndexOf(rule.Preferred, idx, comparison) == idx)
                {
                    searchStart = idx + 1; // continue searching for other blocked occurrences
                    continue;
                }

                return ValidationResult.Violation(rule);
            }
        }

        return ValidationResult.Success;
    }
}
