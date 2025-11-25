namespace BlueDotBrigade.Analyzers.Diagnostics;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using BlueDotBrigade.Analyzers.Dsl;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// BDB001 flags identifiers whose names contain a blocked term (from DSL XML),
/// and suggests the preferred term where applicable.
///
/// Notes:
/// - No default in-memory DSL is used. If no DSL file is found, the analyzer warns (BDB000) and runs with no rules.
/// - DSL filename can be overridden by AnalyzerConfig/MSBuild: build_property.AnalyzerDslFileName (default: "dsl.config.xml").
/// - If multiple DSL files are present, the one under MSBuildProjectDirectory is preferred over solution-level.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DslTerminologyAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "BDB001";
    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Blocked term in identifier",
        messageFormat: "Identifier '{0}' contains blocked term '{1}'.{2}",
        category: "Naming",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Identifiers should not contain blocked terms. Use the preferred term instead where applicable.");

    private static readonly DiagnosticDescriptor MissingConfigRule = new(
        id: "BDB000",
        title: "DSL configuration not found",
        messageFormat: "Expected DSL file '{0}' not found. Analyzer will run with empty rules. Example DSL: {1}",
        category: "Configuration",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The analyzer did not find the DSL configuration file and will not flag any identifiers. Provide a DSL file to enable checks.",
        customTags: new[] { "CompilationEnd" });

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule, MissingConfigRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(startCtx =>
        {
            var targetFileName = GetTargetFileName(startCtx.Options);
            var projectDir = GetProjectDirectory(startCtx.Options);
            var selected = SelectDslAdditionalText(startCtx.Options, targetFileName, projectDir);

            var rules = new List<RuleDef>();
            var hasConfig = false;

            if (selected is not null)
            {
                var text = selected.GetText();
                if (text is not null)
                {
                    try
                    {
                        var doc = XDocument.Parse(text.ToString());
                        rules = ParseDsl(doc);
                        hasConfig = true;
                    }
                    catch
                    {
                        // invalid XML => treat as missing
                        hasConfig = false;
                    }
                }
            }

            if (!hasConfig)
            {
                startCtx.RegisterCompilationEndAction(endCtx =>
                {
                    var diag = Diagnostic.Create(MissingConfigRule, Location.None, targetFileName, DslDefaults.DefaultDslXml);
                    endCtx.ReportDiagnostic(diag);
                });
            }

            // Symbols
            startCtx.RegisterSymbolAction(symbolCtx =>
            {
                if (rules.Count == 0) return;

                var symbol = symbolCtx.Symbol;
                if (string.IsNullOrEmpty(symbol.Name) || symbol.Locations.Length == 0)
                    return;

                // Avoid duplicate diagnostics for properties: skip accessor methods.
                if (symbol is IMethodSymbol m && (m.MethodKind == MethodKind.PropertyGet || m.MethodKind == MethodKind.PropertySet))
                {
                    return;
                }

                CheckAndReport(symbolCtx.ReportDiagnostic, symbol.Locations[0], symbol.Name, rules);

            }, SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Field, SymbolKind.Property, SymbolKind.Parameter);

            // Locals (skip fields; handled by symbol action)
            startCtx.RegisterSyntaxNodeAction(syntaxCtx =>
            {
                if (rules.Count == 0) return;

                var declarator = (VariableDeclaratorSyntax)syntaxCtx.Node;
                if (declarator.Parent is VariableDeclarationSyntax vd && vd.Parent is FieldDeclarationSyntax)
                    return;

                var name = declarator.Identifier.ValueText;
                if (string.IsNullOrWhiteSpace(name))
                    return;

                CheckAndReport(syntaxCtx.ReportDiagnostic, declarator.Identifier.GetLocation(), name, rules);

            }, SyntaxKind.VariableDeclarator);
        });
    }

    private static string GetTargetFileName(AnalyzerOptions options)
    {
        options.AnalyzerConfigOptionsProvider.GlobalOptions
            .TryGetValue("build_property.AnalyzerDslFileName", out var configuredName);
        return string.IsNullOrWhiteSpace(configuredName) ? DslDefaults.DefaultDslFileName : configuredName.Trim();
    }

    private static string? GetProjectDirectory(AnalyzerOptions options)
    {
        // Preferred: MSBuild-provided project directory from AnalyzerConfig
        if (options.AnalyzerConfigOptionsProvider.GlobalOptions
            .TryGetValue("build_property.MSBuildProjectDirectory", out var projectDirRaw))
        {
            var normalized = Normalize(projectDirRaw);
            if (!string.IsNullOrWhiteSpace(normalized))
                return normalized;
        }

        // Fallback for unit tests: allow specifying via an .editorconfig AdditionalFile
        var editorConfig = options.AdditionalFiles
            .FirstOrDefault(f => string.Equals(Path.GetFileName(f.Path), ".editorconfig", StringComparison.OrdinalIgnoreCase));

        var text = editorConfig?.GetText();
        if (text is not null)
        {
            foreach (var line in text.Lines)
            {
                var s = line.ToString();
                if (s.Contains("build_property.MSBuildProjectDirectory", StringComparison.Ordinal))
                {
                    var parts = s.Split('=');
                    if (parts.Length == 2)
                    {
                        var candidate = Normalize(parts[1]);
                        if (!string.IsNullOrWhiteSpace(candidate))
                            return candidate;
                    }
                }
            }
        }

        return null;
    }

    private static AdditionalText? SelectDslAdditionalText(AnalyzerOptions options, string targetFileName, string? projectDir)
    {
        var candidates = options.AdditionalFiles
            .Where(f => string.Equals(Path.GetFileName(f.Path), targetFileName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (candidates.Count == 0)
            return null;

        if (!string.IsNullOrWhiteSpace(projectDir))
        {
            var normalizedProjectDir = Normalize(projectDir);
            if (!string.IsNullOrWhiteSpace(normalizedProjectDir))
            {
                var proj = candidates.FirstOrDefault(f =>
                {
                    var candidateDir = Normalize(Path.GetDirectoryName(f.Path));
                    if (string.IsNullOrWhiteSpace(candidateDir))
                        return false;

                    if (string.Equals(candidateDir, normalizedProjectDir, StringComparison.OrdinalIgnoreCase))
                        return true;

                    var expectedSuffix = Path.DirectorySeparatorChar + normalizedProjectDir;
                    return candidateDir.EndsWith(expectedSuffix, StringComparison.OrdinalIgnoreCase);
                });

                if (proj is not null)
                    return proj; // project-level wins
            }
        }

        // Otherwise return any (e.g., solution-level)
        return candidates[0];
    }

    private static string? Normalize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return path;
        var p = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        var trimmed = p.Trim();
        return Path.TrimEndingDirectorySeparator(trimmed);
    }

    private sealed record RuleDef(string Blocked, string Preferred, bool CaseSensitive);

    private static void CheckAndReport(Action<Diagnostic> report, Location location, string identifierName, List<RuleDef> rules)
    {
        foreach (var r in rules)
        {
            var comparison = r.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            // Fast-path: if the identifier is exactly the preferred term, do not report
            if (!string.IsNullOrEmpty(r.Preferred) && string.Equals(identifierName, r.Preferred, comparison))
            {
                continue;
            }

            var searchStart = 0;
            while (true)
            {
                var idx = identifierName.IndexOf(r.Blocked, searchStart, comparison);
                if (idx < 0)
                {
                    break;
                }

                // If this blocked occurrence aligns with the preferred term at the same position,
                // treat it as allowed (e.g., "Customer" contains "Cust" at index 0 but is preferred)
                if (!string.IsNullOrEmpty(r.Preferred)
                    && idx + r.Preferred.Length <= identifierName.Length
                    && identifierName.IndexOf(r.Preferred, idx, comparison) == idx)
                {
                    searchStart = idx + 1; // continue searching for other blocked occurrences
                    continue;
                }

                var suffix = r.Preferred is null ? string.Empty : $" Instead, use: '{r.Preferred}'";
                var diag = Diagnostic.Create(Rule, location, identifierName, r.Blocked, suffix);
                report(diag);
                return; // one diagnostic per identifier
            }
        }
    }

    private static List<RuleDef> ParseDsl(XDocument doc)
    {
        var list = new List<RuleDef>();
        var root = doc.Root;
        if (root is null || !string.Equals(root.Name.LocalName, "dsl", StringComparison.OrdinalIgnoreCase))
            return list;

        foreach (var t in root.Elements("term"))
        {
            var prefer = (string)t.Attribute("prefer");
            if (string.IsNullOrWhiteSpace(prefer))
                continue;

            var caseAttr = (string)t.Attribute("case");
            var caseSensitive = !string.Equals(caseAttr, "insensitive", StringComparison.OrdinalIgnoreCase); // default sensitive

            var blockedAttr = (string)t.Attribute("block");
            if (!string.IsNullOrWhiteSpace(blockedAttr))
            {
                list.Add(new RuleDef(blockedAttr!, prefer, caseSensitive));
            }

            foreach (var alias in t.Elements("alias"))
            {
                var blocked = (string)alias.Attribute("block");
                if (!string.IsNullOrWhiteSpace(blocked))
                {
                    list.Add(new RuleDef(blocked!, prefer, caseSensitive));
                }
            }
        }

        return list;
    }
}