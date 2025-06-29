using System.Collections.Generic;
using System.Text;

namespace ArmaBotCs.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IReadOnlyList{T}"/> to enhance formatting and display functionality.
/// </summary>
public static class IReadOnlyListExtension
{
    /// <summary>
    /// Converts the elements of the list to a single formatted string, separated by the specified separator.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to format as a string.</param>
    /// <param name="separator">The string to use as a separator between elements. Defaults to ", ".</param>
    /// <returns>
    /// A string containing all elements of the list separated by the specified separator,
    /// or an empty string if the list is null or empty. If an element is null, "null" is used in its place.
    /// </returns>
    public static string ToFormattedString<T>(this IReadOnlyList<T> list, string separator = ", ")
    {
        if (list == null || list.Count == 0)
            return string.Empty;
        var sb = new StringBuilder();
        foreach (var item in list)
        {
            sb.Append(item?.ToString() ?? "null").Append(separator);
        }

        // Remove the last separator
        if (sb.Length > 0)
            sb.Length -= separator.Length;
        return sb.ToString();
    }
}
