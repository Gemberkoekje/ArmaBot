using System.Collections.Generic;
using System.Text;

namespace ArmaBotCs.Extensions;

public static class IReadOnlyListExtension
{
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
