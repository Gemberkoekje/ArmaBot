using Remora.Rest.Core;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArmaBot.Infrastructure.Converters;

public sealed class SnowflakeJsonConverter : JsonConverter<Snowflake>
{
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

    public override void Write(Utf8JsonWriter writer, Snowflake value, JsonSerializerOptions options)
    {
        // Write as string for compatibility
        writer.WriteStringValue(value.Value.ToString());
    }
}