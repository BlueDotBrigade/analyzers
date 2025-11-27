namespace BlueDotBrigade.Analyzers.Dsl;

using System;
using System.Collections.Generic;

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
    /// <returns>The violated <see cref="TerminologyRule"/> if a blocked term is found; otherwise, <c>null</c>.</returns>
    public TerminologyRule GetViolation(string identifierName)
    {
        if (string.IsNullOrEmpty(identifierName) || _rules.Count == 0)
        {
            return null;
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

                return rule;
            }
        }

        return null;
    }
}
