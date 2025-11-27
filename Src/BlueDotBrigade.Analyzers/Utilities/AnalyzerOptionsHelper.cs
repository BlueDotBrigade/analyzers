namespace BlueDotBrigade.Analyzers.Utilities;

using System;
using System.IO;
using System.Linq;

using BlueDotBrigade.Analyzers.Dsl;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Provides utility methods for working with Roslyn analyzer options,
/// specifically for retrieving configuration values and locating additional files.
/// This class encapsulates the logic for reading build properties and selecting
/// DSL configuration files.
/// </summary>
internal static class AnalyzerOptionsHelper
{
    /// <summary>
    /// Gets the target DSL configuration filename from analyzer options.
    /// </summary>
    /// <param name="options">The analyzer options containing build properties.</param>
    /// <returns>
    /// The configured DSL filename if specified via <c>build_property.AnalyzerDslFileName</c>;
    /// otherwise, returns <see cref="DslDefaults.DefaultDslFileName"/>.
    /// </returns>
    /// <remarks>
    /// The DSL filename can be overridden in the project file or .editorconfig using:
    /// <code>
    /// &lt;PropertyGroup&gt;
    ///   &lt;AnalyzerDslFileName&gt;custom-dsl.xml&lt;/AnalyzerDslFileName&gt;
    /// &lt;/PropertyGroup&gt;
    /// </code>
    /// </remarks>
    public static string GetTargetFileName(AnalyzerOptions options)
    {
        options.AnalyzerConfigOptionsProvider.GlobalOptions
            .TryGetValue("build_property.AnalyzerDslFileName", out var configuredName);

        return string.IsNullOrWhiteSpace(configuredName)
            ? DslDefaults.DefaultDslFileName
            : configuredName.Trim();
    }

    /// <summary>
    /// Gets the MSBuild project directory from analyzer options.
    /// </summary>
    /// <param name="options">The analyzer options containing build properties or additional files.</param>
    /// <returns>
    /// The normalized project directory path if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method first attempts to read the project directory from
    /// <c>build_property.MSBuildProjectDirectory</c>. If not available (e.g., in unit tests),
    /// it falls back to parsing an .editorconfig additional file for the same property.
    /// </remarks>
    public static string? GetProjectDirectory(AnalyzerOptions options)
    {
        // Preferred: MSBuild-provided project directory from AnalyzerConfig
        if (options.AnalyzerConfigOptionsProvider.GlobalOptions
            .TryGetValue("build_property.MSBuildProjectDirectory", out var projectDirRaw))
        {
            var normalized = PathHelper.Normalize(projectDirRaw);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                return normalized;
            }
        }

        // Fallback for unit tests: allow specifying via an .editorconfig AdditionalFile
        var editorConfig = options.AdditionalFiles
            .FirstOrDefault(f => string.Equals(
                Path.GetFileName(f.Path),
                ".editorconfig",
                StringComparison.OrdinalIgnoreCase));

        var text = editorConfig?.GetText();
        if (text is not null)
        {
            foreach (var line in text.Lines)
            {
                var s = line.ToString();
                if (s.IndexOf("build_property.MSBuildProjectDirectory", StringComparison.Ordinal) >= 0)
                {
                    var parts = s.Split('=');
                    if (parts.Length == 2)
                    {
                        var candidate = PathHelper.Normalize(parts[1]);
                        if (!string.IsNullOrWhiteSpace(candidate))
                        {
                            return candidate;
                        }
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Selects the appropriate DSL configuration file from the available additional files.
    /// </summary>
    /// <param name="options">The analyzer options containing additional files.</param>
    /// <param name="targetFileName">The name of the DSL file to search for.</param>
    /// <param name="projectDir">The project directory path, used to prefer project-level files.</param>
    /// <returns>
    /// The selected DSL additional text file, or <c>null</c> if no matching file is found.
    /// </returns>
    /// <remarks>
    /// This method implements a priority-based selection:
    /// <list type="number">
    ///   <item>Project-level DSL files (in <paramref name="projectDir"/>) take precedence.</item>
    ///   <item>Solution-level DSL files are used as a fallback.</item>
    /// </list>
    /// This allows projects to override solution-wide DSL rules with project-specific ones.
    /// </remarks>
    public static AdditionalText? SelectDslAdditionalText(
        AnalyzerOptions options,
        string targetFileName,
        string? projectDir)
    {
        var candidates = options.AdditionalFiles
            .Where(f => string.Equals(
                Path.GetFileName(f.Path),
                targetFileName,
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(projectDir))
        {
            var normalizedProjectDir = PathHelper.Normalize(projectDir);
            if (!string.IsNullOrWhiteSpace(normalizedProjectDir))
            {
                var proj = candidates.FirstOrDefault(f =>
                {
                    var candidateDir = PathHelper.Normalize(Path.GetDirectoryName(f.Path));
                    if (string.IsNullOrWhiteSpace(candidateDir))
                    {
                        return false;
                    }

                    if (string.Equals(candidateDir, normalizedProjectDir, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    var expectedSuffix = Path.DirectorySeparatorChar + normalizedProjectDir;
                    return candidateDir.EndsWith(expectedSuffix, StringComparison.OrdinalIgnoreCase);
                });

                if (proj is not null)
                {
                    return proj; // project-level wins
                }
            }
        }

        // Otherwise return any (e.g., solution-level)
        return candidates[0];
    }
}
