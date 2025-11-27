namespace BlueDotBrigade.Analyzers.Utilities;

using System.IO;

/// <summary>
/// Provides utility methods for normalizing and manipulating file system paths.
/// This class encapsulates common path operations used by analyzers for consistent
/// cross-platform path handling.
/// </summary>
internal static class PathHelper
{
    /// <summary>
    /// Normalizes a file system path by replacing alternative directory separator characters
    /// with the standard directory separator and trimming trailing separators.
    /// </summary>
    /// <param name="path">The path to normalize. May be null or whitespace.</param>
    /// <returns>
    /// The normalized path, or the original path if it is null or whitespace.
    /// Trailing directory separators are removed unless the path is a root path.
    /// </returns>
    /// <example>
    /// <code>
    /// var normalized = PathHelper.Normalize("C:/folder/subfolder/");
    /// // Returns "C:\folder\subfolder" on Windows
    /// </code>
    /// </example>
    public static string? Normalize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        var replaced = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        var trimmed = replaced.Trim();
        return TrimEndingDirectorySeparator(trimmed);
    }

    /// <summary>
    /// Removes trailing directory separator characters from a path, unless the path
    /// represents a root directory (e.g., "C:\" on Windows or "/" on Unix).
    /// </summary>
    /// <param name="path">The path from which to remove trailing separators. May be null or empty.</param>
    /// <returns>
    /// The path with trailing directory separators removed, or the original path
    /// if it represents a root directory or is null/empty.
    /// </returns>
    /// <remarks>
    /// This method is compatible with .NET Standard 2.0 which does not have
    /// <c>Path.TrimEndingDirectorySeparator</c>.
    /// </remarks>
    public static string? TrimEndingDirectorySeparator(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        var root = Path.GetPathRoot(path);
        var result = path;
        while (result.Length > 0
               && IsDirectorySeparator(result[result.Length - 1])
               && !string.Equals(result, root, System.StringComparison.Ordinal))
        {
            result = result.Substring(0, result.Length - 1);
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified character is a directory separator
    /// (either the standard or alternative separator for the current platform).
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>
    /// <c>true</c> if the character is a directory separator; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsDirectorySeparator(char c)
    {
        return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
    }
}
