using System;

namespace ArmaBot.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="Guid"/> values.
/// </summary>
public static class GuidExtension
{
    /// <summary>
    /// Returns a new <see cref="Guid"/> instance parsed from the string representation of the current <see cref="Guid"/>.
    /// </summary>
    /// <param name="Guid">The <see cref="Guid"/> to convert.</param>
    /// <returns>
    /// A new <see cref="Guid"/> instance with the same value as the input.
    /// </returns>
    public static Guid ToGuid(this Guid Guid)
    {
        return Guid.Parse(Guid.ToString());
    }
}
