namespace BlueDotBrigade.Analyzers.Dsl;

/// <summary>
/// Represents a single terminology rule that specifies a blocked term and its preferred replacement.
/// </summary>
/// <remarks>
/// This class is used to define naming conventions where certain terms are blocked and
/// should be replaced with preferred alternatives. For example, blocking "Cust" and
/// preferring "Customer".
/// </remarks>
public sealed class TerminologyRule
{
    /// <summary>
    /// Gets the term that should be blocked (not allowed in identifiers).
    /// </summary>
    public string Blocked { get; }

    /// <summary>
    /// Gets the preferred term that should be used instead of the blocked term.
    /// </summary>
    public string Preferred { get; }

    /// <summary>
    /// Gets a value indicating whether the comparison should be case-sensitive.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, "cust" and "Cust" are treated as different terms.
    /// When <c>false</c>, they are treated as the same term.
    /// </remarks>
    public bool CaseSensitive { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TerminologyRule"/> class.
    /// </summary>
    /// <param name="blocked">The term that should be blocked.</param>
    /// <param name="preferred">The preferred term to use instead.</param>
    /// <param name="caseSensitive">Whether the comparison should be case-sensitive.</param>
    public TerminologyRule(string blocked, string preferred, bool caseSensitive)
    {
        Blocked = blocked;
        Preferred = preferred;
        CaseSensitive = caseSensitive;
    }
}
