namespace WindowsSearchConfigurator.Utilities;

using System.Text.RegularExpressions;

/// <summary>
/// Provides wildcard pattern matching functionality.
/// </summary>
public class WildcardMatcher
{
    /// <summary>
    /// Converts a wildcard pattern to a regular expression.
    /// </summary>
    /// <param name="pattern">The wildcard pattern (e.g., *.txt, test?.log).</param>
    /// <returns>A <see cref="Regex"/> object for matching.</returns>
    public static Regex ConvertToRegex(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Pattern cannot be null or empty.", nameof(pattern));
        }

        // Escape special regex characters except * and ?
        var regexPattern = Regex.Escape(pattern)
            .Replace(@"\*", ".*")      // * matches zero or more characters
            .Replace(@"\?", ".");      // ? matches exactly one character

        // Match from start to end
        regexPattern = $"^{regexPattern}$";

        return new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    /// <summary>
    /// Checks if a value matches a wildcard pattern.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <param name="pattern">The wildcard pattern.</param>
    /// <returns>True if the value matches the pattern; otherwise, false.</returns>
    public static bool IsMatch(string value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var regex = ConvertToRegex(pattern);
        return regex.IsMatch(value);
    }

    /// <summary>
    /// Filters a collection based on a wildcard pattern.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="items">The collection to filter.</param>
    /// <param name="pattern">The wildcard pattern.</param>
    /// <param name="selector">A function to extract the string value to match against.</param>
    /// <returns>A filtered collection containing only matching items.</returns>
    public static IEnumerable<T> Filter<T>(IEnumerable<T> items, string pattern, Func<T, string> selector)
    {
        var regex = ConvertToRegex(pattern);
        return items.Where(item => regex.IsMatch(selector(item)));
    }

    /// <summary>
    /// Checks if a pattern contains wildcard characters.
    /// </summary>
    /// <param name="pattern">The pattern to check.</param>
    /// <returns>True if the pattern contains * or ?; otherwise, false.</returns>
    public static bool ContainsWildcards(string pattern)
    {
        return !string.IsNullOrWhiteSpace(pattern) && (pattern.Contains('*') || pattern.Contains('?'));
    }
}
