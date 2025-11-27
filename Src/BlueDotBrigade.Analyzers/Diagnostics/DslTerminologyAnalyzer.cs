namespace BlueDotBrigade.Analyzers.Diagnostics;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using BlueDotBrigade.Analyzers.Dsl;
using BlueDotBrigade.Analyzers.Utilities;

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
            var targetFileName = AnalyzerOptionsHelper.GetTargetFileName(startCtx.Options);
            var projectDir = AnalyzerOptionsHelper.GetProjectDirectory(startCtx.Options);
            var selected = AnalyzerOptionsHelper.SelectDslAdditionalText(startCtx.Options, targetFileName, projectDir);

            var rules = new List<TerminologyRule>();
            var hasConfig = false;

            if (selected is not null)
            {
                var text = selected.GetText();
                if (text is not null)
                {
                    try
                    {
                        rules = DslRuleParser.Parse(text.ToString());
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

            var validator = new TerminologyValidator(rules);

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

                ReportIfViolation(symbolCtx.ReportDiagnostic, symbol.Locations[0], symbol.Name, validator);

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

                ReportIfViolation(syntaxCtx.ReportDiagnostic, declarator.Identifier.GetLocation(), name, validator);

            }, SyntaxKind.VariableDeclarator);
        });
    }

    private static void ReportIfViolation(Action<Diagnostic> report, Location location, string identifierName, TerminologyValidator validator)
    {
        var violatedRule = validator.GetViolation(identifierName);
        if (violatedRule is not null)
        {
            var suffix = violatedRule.Preferred is null ? string.Empty : $" Instead, use: '{violatedRule.Preferred}'";
            var diag = Diagnostic.Create(Rule, location, identifierName, violatedRule.Blocked, suffix);
            report(diag);
        }
    }
}
