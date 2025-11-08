using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BlueDotBrigade.Analyzers.Diagnostics
{
    /// <summary>
    /// RC001 flags identifiers whose names contain a blocked term (from DSL XML),
    /// and suggests the preferred term where applicable.
    ///
    /// XML schema (supported):
    /// <dsl>
    ///   <term prefer="Customer" block="Client" case="sensitive"/>
    ///   <term prefer="Customer" case="sensitive">
    ///     <alias block="Client"/>
    ///     <alias block="Cust"/>
    ///   </term>
    /// </dsl>
    ///
    /// Notes:
    /// - If 'case' is omitted, behavior defaults to "sensitive".
    /// - If no XML AdditionalFile is found, a default sample DSL (above) is synthesized and used.
    /// - AdditionalFiles filename can be overridden by MSBuild property:
    ///     build_property.AnalyzerDslFileName (default: "dsl.config.xml")
    /// - Project-local file is preferred when MSBuildProjectDirectory is visible.
    /// - Fallback: if MSBuildProjectDirectory is unavailable (e.g., analyzer config cannot be added in test
    ///   harness), we attempt to infer the intended project directory from an AdditionalFile named ".editorconfig"
    ///   by parsing 'build_property.MSBuildProjectDirectory = <value>'. This allows tests to supply the setting
    ///   via AdditionalFiles when Solution.AddAnalyzerConfigDocument is missing.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DslTerminologyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RC001";
        private const string DefaultDslFileName = "dsl.config.xml";

        private static readonly string DefaultDslXml = """
        <dsl>
          <term prefer="Customer" block="Client" case="sensitive"/>
          <term prefer="Customer" case="sensitive">
            <alias block="Client"/>
            <alias block="Cust"/>
          </term>
        </dsl>
        """;

        private static readonly LocalizableString Title =
            "Blocked term in identifier";

        private static readonly LocalizableString MessageFormat =
            // {0}=identifier, {1}=blocked, {2}=optional " Use 'preferred' instead."
            "Identifier '{0}' contains blocked term '{1}'.{2}";

        private static readonly LocalizableString Description =
            "Identifiers should not contain blocked terms. Use the preferred term instead where applicable.";

        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new(
            id: DiagnosticId,
            title: Title,
            messageFormat: MessageFormat,
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(startCtx =>
            {
                var rules = LoadRules(startCtx.Options);
                if (rules.Count == 0)
                {
                    // No config found or empty; use built-in default DSL.
                    rules = ParseDsl(XDocument.Parse(DefaultDslXml));
                }

                startCtx.RegisterSymbolAction(symbolCtx =>
                {
                    var symbol = symbolCtx.Symbol;
                    if (string.IsNullOrEmpty(symbol.Name) || symbol.Locations.Length == 0)
                        return;

                    CheckAndReport(symbolCtx.ReportDiagnostic, symbol.Locations[0], symbol.Name, rules);

                }, SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Field, SymbolKind.Property, SymbolKind.Parameter);

                // Locals
                startCtx.RegisterSyntaxNodeAction(syntaxCtx =>
                {
                    var declarator = (VariableDeclaratorSyntax)syntaxCtx.Node;

                    // Skip fields (handled by symbol action)
                    if (declarator.Parent is VariableDeclarationSyntax vd &&
                        vd.Parent is FieldDeclarationSyntax)
                    {
                        return;
                    }

                    var name = declarator.Identifier.ValueText;
                    if (string.IsNullOrWhiteSpace(name))
                        return;

                    CheckAndReport(syntaxCtx.ReportDiagnostic, declarator.Identifier.GetLocation(), name, rules);

                }, SyntaxKind.VariableDeclarator);
            });
        }

        private sealed record RuleDef(string Blocked, string Preferred, bool CaseSensitive);

        private static void CheckAndReport(Action<Diagnostic> report, Location location, string identifierName, List<RuleDef> rules)
        {
            foreach (var r in rules)
            {
                var comparison = r.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                if (identifierName.IndexOf(r.Blocked, comparison) >= 0)
                {
                    var suffix = r.Preferred is null ? string.Empty : $" Use '{r.Preferred}' instead.";
                    var diag = Diagnostic.Create(Rule, location, identifierName, r.Blocked, suffix);
                    report(diag);
                    return; // one diagnostic per identifier
                }
            }
        }

        private static List<RuleDef> LoadRules(AnalyzerOptions options)
        {
            // Optional override for file name
            options.AnalyzerConfigOptionsProvider.GlobalOptions
                .TryGetValue("build_property.AnalyzerDslFileName", out var configuredName);
            var targetFileName = string.IsNullOrWhiteSpace(configuredName) ? DefaultDslFileName : configuredName.Trim();

            // Optional project directory (to prefer project-local file)
            options.AnalyzerConfigOptionsProvider.GlobalOptions
                .TryGetValue("build_property.MSBuildProjectDirectory", out var projectDirRaw);
            var projectDir = projectDirRaw?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Fallback: attempt to parse .editorconfig from AdditionalFiles if property absent
            if (string.IsNullOrEmpty(projectDir))
            {
                var editorConfig = options.AdditionalFiles.FirstOrDefault(f => string.Equals(Path.GetFileName(f.Path), ".editorconfig", StringComparison.OrdinalIgnoreCase));
                if (editorConfig is not null)
                {
                    var text = editorConfig.GetText();
                    if (text is not null)
                    {
                        foreach (var line in text.Lines)
                        {
                            var value = line.ToString();
                            if (value.Contains("build_property.MSBuildProjectDirectory", StringComparison.Ordinal))
                            {
                                var parts = value.Split('=');
                                if (parts.Length == 2)
                                {
                                    var candidate = parts[1].Trim();
                                    if (!string.IsNullOrWhiteSpace(candidate))
                                    {
                                        projectDir = candidate.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Normalize separators so comparisons work with either '/' or '\\'
            projectDir = NormalizePath(projectDir);

            var candidates = options.AdditionalFiles
                .Where(f => string.Equals(Path.GetFileName(f.Path), targetFileName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // If none found, return empty so caller injects default DSL
            if (candidates.Count == 0)
                return new();

            AdditionalText chosen = candidates[0];
            if (!string.IsNullOrEmpty(projectDir))
            {
                var projectLocal = candidates.FirstOrDefault(f =>
                {
                    var dir = NormalizePath(Path.GetDirectoryName(f.Path));
                    return string.Equals(dir, projectDir, StringComparison.OrdinalIgnoreCase);
                });
                if (projectLocal is not null) chosen = projectLocal;
            }
            else if (candidates.Count > 1)
            {
                // Heuristic when project dir cannot be determined: prefer deepest path (most directory separators)
                chosen = candidates
                    .OrderByDescending(f => f.Path.Count(ch => ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar))
                    .First();
            }

            var textChosen = chosen.GetText();
            if (textChosen is null) return new();

            try
            {
                var doc = XDocument.Parse(textChosen.ToString());
                return ParseDsl(doc);
            }
            catch
            {
                return new();
            }
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;
            var normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            return normalized.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        /// <summary>
        /// Parse the v3 DSL:
        /// - One-off: <term prefer="X" block="Y" case="sensitive|insensitive"/>
        /// - One-to-many:
        ///     <term prefer="X" case="..."><alias block="Y1"/><alias block="Y2"/></term>
        /// Rules:
        /// - If 'case' omitted on a term, default = sensitive.
        /// - Child aliases inherit the 'case' from the parent term.
        /// </summary>
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

                // one-off attribute form
                var blockedAttr = (string)t.Attribute("block");
                if (!string.IsNullOrWhiteSpace(blockedAttr))
                {
                    list.Add(new RuleDef(blockedAttr!, prefer, caseSensitive));
                }

                // child alias elements
                foreach (var alias in t.Elements("alias"))
                {
                    var blocked = (string)alias.Attribute("block");
                    if (!string.IsNullOrWhiteSpace(blocked))
                    {
                        list.Add(new RuleDef(blocked!, prefer, caseSensitive)); // inherits parent's case
                    }
                }
            }

            return list;
        }
    }
}
