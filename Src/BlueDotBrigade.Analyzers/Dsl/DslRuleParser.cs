namespace BlueDotBrigade.Analyzers.Dsl;

using System;
using System.Collections.Generic;
using System.Xml.Linq;

/// <summary>
/// Parses DSL XML configuration files to extract terminology rules.
/// </summary>
/// <remarks>
/// The DSL XML format supports defining preferred terms with blocked alternatives:
/// <code>
/// &lt;dsl&gt;
///   &lt;term prefer="Customer" block="Client" case="sensitive"/&gt;
///   &lt;term prefer="Customer" case="sensitive"&gt;
///     &lt;alias block="Client"/&gt;
///     &lt;alias block="Cust"/&gt;
///   &lt;/term&gt;
/// &lt;/dsl&gt;
/// </code>
/// </remarks>
public static class DslRuleParser
{
    /// <summary>
    /// Parses DSL XML content and returns a list of terminology rules.
    /// </summary>
    /// <param name="xmlContent">The XML content to parse.</param>
    /// <returns>A list of <see cref="TerminologyRule"/> objects parsed from the XML.</returns>
    /// <exception cref="System.Xml.XmlException">Thrown when the XML content is invalid.</exception>
    public static List<TerminologyRule> Parse(string xmlContent)
    {
        var doc = XDocument.Parse(xmlContent);
        return ParseDocument(doc);
    }

    /// <summary>
    /// Parses an XDocument and returns a list of terminology rules.
    /// </summary>
    /// <param name="doc">The XDocument to parse.</param>
    /// <returns>A list of <see cref="TerminologyRule"/> objects parsed from the document.</returns>
    public static List<TerminologyRule> ParseDocument(XDocument doc)
    {
        var list = new List<TerminologyRule>();
        var root = doc.Root;

        if (root is null || !string.Equals(root.Name.LocalName, "dsl", StringComparison.OrdinalIgnoreCase))
        {
            return list;
        }

        foreach (var t in root.Elements("term"))
        {
            var prefer = (string)t.Attribute("prefer");
            if (string.IsNullOrWhiteSpace(prefer))
            {
                continue;
            }

            var caseAttr = (string)t.Attribute("case");
            var caseSensitive = !string.Equals(caseAttr, "insensitive", StringComparison.OrdinalIgnoreCase);

            var blockedAttr = (string)t.Attribute("block");
            if (!string.IsNullOrWhiteSpace(blockedAttr))
            {
                list.Add(new TerminologyRule(blockedAttr, prefer, caseSensitive));
            }

            foreach (var alias in t.Elements("alias"))
            {
                var blocked = (string)alias.Attribute("block");
                if (!string.IsNullOrWhiteSpace(blocked))
                {
                    list.Add(new TerminologyRule(blocked, prefer, caseSensitive));
                }
            }
        }

        return list;
    }
}
