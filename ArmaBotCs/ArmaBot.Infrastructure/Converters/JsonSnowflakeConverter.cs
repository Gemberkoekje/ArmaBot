using Remora.Rest.Core;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArmaBot.Infrastructure.Converters;

/// <summary>
/// Provides a custom JSON converter for the <see cref="Snowflake"/> type, enabling correct serialization and deserialization
/// of Discord snowflake identifiers as strings or numbers.
/// </summary>
public sealed class SnowflakeJsonConverter : JsonConverter<Snowflake>
{
    /// <summary>
    /// Reads and converts the JSON representation of a Discord snowflake to a <see cref="Snowflake"/> instance.
    /// Accepts both string and numeric representations.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type to convert (should be <see cref="Snowflake"/>).</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>A <see cref="Snowflake"/> instance parsed from the JSON value.</returns>
    /// <exception cref="JsonException">Thrown if the value cannot be parsed as a valid snowflake.</exception>
    public override Snowflake Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Discord snowflakes are usually serialized as strings
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (ulong.TryParse(str, out var value))
            {
                return new Snowflake(value);
            }
            throw new JsonException($"Invalid Snowflake string: {str}");
        }
        // Or as numbers
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetUInt64(out var ulongValue))
        {
            return new Snowflake(ulongValue);
        }
        throw new JsonException("Invalid token for Snowflake");
    }

    /// <summary>
    /// Writes a <see cref="Snowflake"/> value as a JSON string for compatibility with Discord APIs.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The <see cref="Snowflake"/> value to write.</param>
    /// <param name="options">The serializer options to use.</param>
    public override void Write(Utf8JsonWriter writer, Snowflake value, JsonSerializerOptions options)
    {
        // Write as string for compatibility
        writer.WriteStringValue(value.Value.ToString());
    }
}
